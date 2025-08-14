// Get the HTML element where the count will be displayed
const visitCountElement = document.querySelector("#visitor-count");

// Asynchronous function to fetch and display the visit count from Azure Function
async function updateVisitCount() {
  try {
    // Make a network request to your Azure Function URL.
    // The `fetch` function returns a Promise.
    const response = await fetch();
    //"function url"

    // Parse the JSON response from the function to get the new count.
    const newCount = await response.json();

    // Update the text content of the HTML element with the new count.
    visitCountElement.textContent = `Visitor Count: ${newCount}`;
  } catch (error) {
    // If the fetch request fails, log the error and display an error message.
    console.error("Error fetching visit count:", error);
    visitCountElement.textContent = "Error loading count.";
  }
}

// Call the function to run the process when the page loads.
updateVisitCount();
