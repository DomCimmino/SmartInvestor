from pathlib import Path
import sqlite3
from pulp import LpMaximize, LpProblem, LpVariable, lpSum

db_path = Path(__file__).resolve().parent.parent / "Resources" / "smart_investor.db3" 

if not db_path.exists():
    print(f"Database not found: {db_path}")
else:
    print(f"Database: {db_path}")

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

query = """
SELECT * FROM CompanyDtos
WHERE MarketCap > 100000000 
  AND CurrentRatio > 2 
  AND PriceBookValue < 2.5 
  AND PriceEarningsRatio < 15
"""
cursor.execute(query)
companies = cursor.fetchall()
conn.close()

problem = LpProblem("Company_Optimization", LpMaximize)

var_dict = {company[0]: LpVariable(f"select_{company[0]}", cat="Binary") for company in companies}

problem += lpSum([
    var_dict[company[0]] * (
        company[7] +
        company[8] +
        company[9]  
    ) for company in companies
])

problem.solve()

selected_companies = [company for company in companies if var_dict[company[0]].value() == 1]
for company in selected_companies:
    print(f"Azienda selezionata: {company[1]} (Ticker: {company[2]})")
