const visitCountElement = document.querySelector("#visitor-count");

async function updateVisitCount() {
  try {
    // Replace with your Azure Function URL
    const response = await fetch(
      "https://teshwar-c-sharp.azurewebsites.net/api/visitCount?"
    );

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    // Parse the JSON body
    const data = await response.json();

    // Make sure the data has the "count" property
    if (!data || typeof data.count !== "number") {
      throw new Error("Invalid data format from server.");
    }

    // Update visitor count in HTML
    visitCountElement.textContent = `Visitor Count: ${data.count}`;
  } catch (error) {
    console.error("Error fetching visit count:", error);
    visitCountElement.textContent = "Error loading count.";
  }
}

// Run when page loads
updateVisitCount();
