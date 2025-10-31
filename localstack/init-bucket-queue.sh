#!/bin/bash
set -e

# ------------------------------------------------------------
# Wait until LocalStack is fully initialized and ready
# ------------------------------------------------------------
echo "Waiting for LocalStack to be ready..."
echo "Waiting for LocalStack SQS service to be available..."

# Keep checking until the SQS service responds successfully
until awslocal sqs list-queues >/dev/null 2>&1; do
  sleep 2
done

# ------------------------------------------------------------
# Create an S3 bucket to simulate AWS S3
# ------------------------------------------------------------
# This bucket will be used to store uploaded files.
# When a file is uploaded, an event notification will be triggered.
awslocal s3 mb s3://my-bucket
echo "S3 bucket 'my-bucket' created."

# ------------------------------------------------------------
# Create an SQS queue to receive S3 event notifications
# ------------------------------------------------------------
# This queue will be linked to the S3 bucket.
# When an object is created in S3, a message will be sent to this queue.
awslocal sqs create-queue --queue-name my-queue
echo "SQS queue 'my-queue' created."

# ------------------------------------------------------------
# Retrieve the ARN (Amazon Resource Name) of the created queue
# ------------------------------------------------------------
# We need this ARN to link the queue to the S3 bucket notification.
QUEUE_ARN=$(awslocal sqs get-queue-attributes \
  --queue-url http://localhost:4566/000000000000/my-queue \
  --attribute-names QueueArn \
  --query 'Attributes.QueueArn' \
  --output text)

# ------------------------------------------------------------
# Configure S3 to send notifications to the SQS queue
# ------------------------------------------------------------
# This tells S3 to send an event message to 'my-queue' whenever
# a new object is created in 'my-bucket' (e.g., file upload).
awslocal s3api put-bucket-notification-configuration \
  --bucket my-bucket \
  --notification-configuration "{
    \"QueueConfigurations\": [
      {
        \"QueueArn\": \"$QUEUE_ARN\",
        \"Events\": [\"s3:ObjectCreated:*\"]
      }
    ]
  }"

# Adding Queue Policy
awslocal sqs set-queue-attributes \
  --queue-url http://localhost:4566/000000000000/my-queue \
  --attributes '{
    "Policy": "{\"Version\":\"2012-10-17\",\"Statement\":[{\"Effect\":\"Allow\",\"Principal\":\"*\",\"Action\":\"sqs:SendMessage\",\"Resource\":\"arn:aws:sqs:us-east-1:000000000000:my-queue\",\"Condition\":{\"ArnEquals\":{\"aws:SourceArn\":\"arn:aws:s3:::my-bucket\"}}}]}"
  }'

echo "S3 bucket 'my-bucket' successfully linked to SQS queue 'my-queue'."
echo "LocalStack initialization complete."
