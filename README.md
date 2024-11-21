# SmartInvestor

SmartInvestor is a hybrid application combining **C#** and **Python** to retrieve, process, optimize, and visualize stock performance data. It leverages **SEC** filings for financial data, **SQLite** for efficient querying, and Python for optimization and visualization.

## Features

- **Data Retrieval**:
  - Downloads financial data from the **SEC EDGAR API**.
  - Extracts key metrics like earnings per share (EPS), dividends, market cap, and growth rates.

- **C# Module**:
  - Loads SEC data into an **SQLite database** for efficient querying.
  - Filters companies based on custom financial criteria.
  - Exports filtered data to Python for further optimization and visualization.

- **Python Module**:
  - Performs discrete optimization using the **Branch and Bound** algorithm.
  - Visualizes stock performance using historical data from **Yahoo Finance**.
  - Calculates growth percentages and investment projections from specific dates.

## Requirements

### C# Component

- .NET SDK 6.0 or later
- SQLite database
- SEC EDGAR API access

### Python Component

- Python 3.8 or later
- Required libraries:
  - `yfinance`
  - `matplotlib`
  - `numpy`
  - `scipy` (or another library for optimization)
