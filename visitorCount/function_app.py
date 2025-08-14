import azure.functions as func
import logging

# Initialize the Function App with the specified HTTP authorization level
app = func.FunctionApp(http_auth_level=func.AuthLevel.FUNCTION)

# Define the HTTP trigger for the function
@app.route(route="getVisitorCount")
# Define the Cosmos DB input binding to read the 'visit_counter' document
# This binding connects to 'visits-db' database and 'visits' container.
# It specifically targets the document with 'id' set to 'visit_counter'.
# The 'partition_key' is also set to 'visit_counter', assuming 'id' is your partition key.
@app.cosmos_db_input(arg_name="inputDocument", 
                     database_name="visits-db", 
                     container_name="visits", 
                     id="visit_counter", 
                     partition_key="visit_counter", 
                     connection="CosmosDbConnectionString")
def getVisitorCount(req: func.HttpRequest, inputDocument: func.Document) -> func.HttpResponse:
    """
    This function retrieves a visitor count from Cosmos DB and returns it via HTTP.

    Args:
        req (func.HttpRequest): The HTTP request object.
        inputDocument (func.Document): The Cosmos DB document retrieved by the input binding.
                                       This will be the document with id "visit_counter".

    Returns:
        func.HttpResponse: An HTTP response containing the visitor count and 
                           an optional personalized greeting if a name is provided.
    """
    logging.info('Python HTTP trigger function processed a request.')

    # Initialize visitor_count. This will be updated if the document is found.
    visitor_count = 0
    message_prefix = "This HTTP triggered function executed successfully."

    # Check if the inputDocument was successfully retrieved from Cosmos DB
    if inputDocument:
        # Get the 'count' field from the document, defaulting to 0 if not found
        visitor_count = inputDocument.get("count", 0)
        logging.info(f'Retrieved visitor count: {visitor_count}')
        message_prefix = f"Current visitor count: {visitor_count}. {message_prefix}"
    else:
        logging.warning("Document with id 'visit_counter' not found in Cosmos DB.")
        message_prefix = f"Visitor count not found. {message_prefix}"

    # --- Existing logic for personalized response based on 'name' parameter ---
    name = req.params.get('name')
    if not name:
        try:
            req_body = req.get_json()
        except ValueError:
            pass
        else:
            name = req_body.get('name')

    if name:
        # If a name is provided, append a personalized greeting
        return func.HttpResponse(
            f"{message_prefix} Hello, {name}."
        )
    else:
        # If no name is provided, just return the count message
        return func.HttpResponse(
             f"{message_prefix} Pass a name in the query string or in the request body for a personalized response.",
             status_code=200
        )

