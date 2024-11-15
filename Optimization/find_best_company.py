from pathlib import Path
import sqlite3
from pulp import LpMaximize, LpProblem, LpVariable, lpSum
import yfinance as yf
import matplotlib.pyplot as plt
from operator import itemgetter


def plot_stock_history(tickers, top_3_companies):
    plt.figure(figsize=(12, 8))
    for ticker, (_, _, score) in zip(tickers, top_3_companies):
        stock_data = yf.Ticker(ticker).history(start="2019-01-01")
        start_price = stock_data["Close"].iloc[0]
        current_price = stock_data["Close"].iloc[-1]
        growth_percentage = ((current_price - start_price) / start_price) * 100
        plt.plot(stock_data.index, stock_data["Close"], label=f"{ticker} ({growth_percentage:.2f}%)")

    plt.title("Historical performance of the top three selected companies")
    plt.xlabel("Date")
    plt.ylabel("Closing Price")

    ranking_text = "Ranking:\n" + "\n".join([
        f"{i + 1}. {company[0]} (Score: {company[2]:.2f})"
        for i, company in enumerate(top_3_companies)
    ])

    plt.gcf().text(0.02, 0.98, ranking_text, fontsize=10, verticalalignment='top',
                   bbox=dict(facecolor='white', alpha=0.8))

    plt.legend()
    plt.show()


db_path = Path(__file__).resolve().parent.parent / "Resources" / "smart_investor.db3"

if not db_path.exists():
    print(f"Database not found: {db_path}")
else:
    print(f"Database: {db_path}")

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

query = """
SELECT * FROM CompanyDtos
"""
cursor.execute(query)
companies = cursor.fetchall()
conn.close()

problem = LpProblem("Company_Optimization", LpMaximize)

valid_companies = [
    company for company in companies
    if all(value is not None and not isinstance(value, float) or not (value != value or abs(value) == float('inf'))
           for value in [company[3], company[4], company[5], company[6]])
]

var_dict = {company[0]: LpVariable(f"select_{company[0]}", cat="Binary") for company in valid_companies}


for company in valid_companies:
    var = var_dict[company[0]]
    problem += (var * company[4] >= 2 * var), f"CurrentRatio_{company[0]}"  
    problem += (var * company[5] <= 2.5 * var), f"PriceBookValue_{company[0]}"  
    problem += (var * company[6] <= 15 * var), f"PriceEarningsRatio_Upper_{company[0]}" 
    problem += (var * company[3] >= 1e8 * var), f"MarketCap_{company[0]}" 



problem += lpSum([
    var_dict[company[0]] * (
            (company[7] / 100) +  
            company[8] +  
            company[9] 
    ) for company in valid_companies
])

problem.solve()

selected_companies = [
    (company[1], company[2], (company[7] / 100) + company[8] + company[9])
    for company in valid_companies if var_dict[company[0]].value() == 1
]

selected_companies.sort(key=itemgetter(2), reverse=True)

top_3_tickers = [company[1] for company in selected_companies[:3]]

top_3_companies = selected_companies[:3]
top_3_tickers = [company[1] for company in top_3_companies]

plot_stock_history(top_3_tickers, top_3_companies)
