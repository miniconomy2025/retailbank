from locust import HttpUser, task, between, events
import requests
import time
import uuid

shared_account_id = None

@events.test_start.add_listener
def on_test_start(environment):
    """
    This runs ONCE before any users are spawned.
    Starts the simulation, and creates a single shared account.
    """
    print("Load Test is starting - performing global setup")

    global shared_account_id
    base_url = environment.host
    
    print("Calling start sim endpoint")
    start_response = requests.post(f"{base_url}/simulation", json={"epochStartTime": int(time.time())})
    print(f"Start sim response: {start_response.status_code}")
    
    print("Creating shared account")
    account_response = requests.post(f"{base_url}/accounts", json={"salaryCents": 500000})
    
    if account_response.status_code == 200:
        account_number = account_response.json().get("accountId")
        shared_account_id = int(account_number) if account_number else None
        print(f"Shared account created: {shared_account_id}")
    else:
        print(f"Failed to create shared account: {account_response.status_code}")
        shared_account_id = None
    
    print("Global setup complete - ready for testing")

class APIUser(HttpUser):
    """
    Simulates a user who creates their own account, makes transfers to the shared account, and checks reports.
    """

    wait_time = between(2, 3)
    
    def on_start(self):
        """
        Called when a simulated user starts.
        Each user creates their own account first.
        """
        response = self.client.post("/accounts", json={"salaryCents": 500000}, name="Create Account")
        if response.status_code == 200:
            # Save account ID for use in other requests
            account_number = response.json().get("accountId")
            self.account_id = int(account_number) if account_number else None
        else:
            self.account_id = None
    
    @task(5)
    def make_transfer(self):
        """
        Make a transfer from user's account to the shared account.
        Weight: 5 (happens most frequently)
        """
        if not self.account_id:
            print("Cannot make transfer: user account_id is None")
            return
        
        if not shared_account_id:
            print("Cannot make transfer: shared_account_id is None")
            return
        
        transfer_payload = {
            "from": str(self.account_id),
            "to": str(shared_account_id),
            "amountCents": 1,
            "reference": uuid.uuid4().int & (1<<64)-1
        }

        self.client.post("/transfers", json=transfer_payload, name="Make Transfer")
    
    @task(1)
    def fetch_report(self):
        """
        Fetch the report periodically.
        Weight: 1 (happens less frequently than transfers)
        """
        
        self.client.get(f"/report", name="Fetch Report")
