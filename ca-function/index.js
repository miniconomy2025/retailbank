const {
  SecretsManagerClient,
  GetSecretValueCommand,
} = require("@aws-sdk/client-secrets-manager");

const secretsManager = new SecretsManagerClient({
  region: process.env.AWS_REGION || "af-south-1",
});

async function getSecret(secretArn) {
  try {
    const command = new GetSecretValueCommand({ SecretId: secretArn });
    const response = await secretsManager.send(command);
    return response.SecretString;
  } catch (error) {
    console.error(`Error fetching secret ${secretArn}:`, error);
    throw error;
  }
}

exports.handler = async (event) => {
  try {
    const path = event?.rawPath || event?.requestContext?.http?.path || "/";

    if (event?.requestContext?.http?.method === "POST" && path === "/csr") {
      // Fetch secrets from AWS Secrets Manager
      const privateKeySecretArn = process.env.CA_PRIVATE_KEY_SECRET_ARN;
      const certificateSecretArn = process.env.CA_CERTIFICATE_SECRET_ARN;

      if (!privateKeySecretArn || !certificateSecretArn) {
        throw new Error("Secret ARNs not configured in environment variables");
      }

      const [privateKey, certificate] = await Promise.all([
        getSecret(privateKeySecretArn),
        getSecret(certificateSecretArn),
      ]);

      return {
        statusCode: 200,
        headers: {
          "Content-Type": "application/json",
          "Access-Control-Allow-Origin": "*",
          "Access-Control-Allow-Methods": "POST",
          "Access-Control-Allow-Headers": "Content-Type",
        },
        body: JSON.stringify({
          message: "CA Secrets retrieved successfully",
          method: "POST",
          timestamp: new Date().toISOString(),
          secrets: {
            privateKey: privateKey,
            certificate: certificate,
          },
        }),
      };
    }

    return {
      statusCode: 405,
      headers: {
        "Content-Type": "application/json",
        "Access-Control-Allow-Origin": "*",
      },
      body: JSON.stringify({
        error: "Method Not Allowed",
        message: "Only POST requests are supported",
      }),
    };
  } catch (error) {
    console.error("Error:", error);
    return {
      statusCode: 500,
      headers: {
        "Content-Type": "application/json",
        "Access-Control-Allow-Origin": "*",
      },
      body: JSON.stringify({
        error: "Internal Server Error",
        message: error.message,
      }),
    };
  }
};
