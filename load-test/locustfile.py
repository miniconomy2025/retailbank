from locust import HttpUser, task, between, events
import requests
import time
import uuid
import random

all_account_ids = []

@events.test_start.add_listener
def on_test_start(environment):
    """
    This runs ONCE before any users are spawned.
    Starts the simulation.
    """
    print("Load Test is starting - performing global setup")

    base_url = environment.host
    
    print("Calling start sim endpoint")
    start_response = requests.post(f"{base_url}/simulation", json={"epochStartTime": int(time.time())})
    print(f"Start sim response: {start_response.status_code}")
    
    print("Global setup complete - ready for testing")

class APIUser(HttpUser):
    """
    Simulates a user who creates their own account, makes transfers to random other accounts, and checks reports.
    """

    wait_time = between(2, 3)
    
    def on_start(self):
        """
        Called when a simulated user starts.
        Each user creates their own account first.
        """
        global all_account_ids
        
        response = self.client.post("/accounts", json={"salaryCents": 500000}, name="Create Account")
        if response.status_code == 200:
            # Save account ID for use in other requests
            account_number = response.json().get("accountId")
            self.account_id = int(account_number) if account_number else None
            
            # Add account to the global list for others to transfer to
            if self.account_id:
                all_account_ids.append(self.account_id)
        else:
            self.account_id = None
    
    @task(1)
    def make_transfer(self):
        """
        Make a transfer from user's account to a random other account.
        Weight: 1 (happens most frequently)
        """
        if not self.account_id:
            print("Cannot make transfer: user account_id is None")
            return
        
        if len(all_account_ids) < 2:
            print("Cannot make transfer: not enough accounts available")
            return
        
        # Select a random account from all accounts
        target_account_id = random.choice(all_account_ids)
        while target_account_id == self.account_id:
            target_account_id = random.choice(all_account_ids)
        
        transfer_payload = {
            "from": str(self.account_id),
            "to": str(target_account_id),
            "amountCents": 1,
            "reference": uuid.uuid4().int & (1<<64)-1
        }

        self.client.post("/transfers", json=transfer_payload, name="Make Transfer")