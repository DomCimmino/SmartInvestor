import sqlite3
from pathlib import Path


def get_db_path():
    db_path = Path(__file__).resolve().parent.parent / "Resources" / "smart_investor.db3"
    if not db_path.exists():
        raise FileNotFoundError(f"Database not found: {db_path}")
    return db_path


def fetch_companies():
    db_path = get_db_path()
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    query = "SELECT * FROM CompanyDtos"
    cursor.execute(query)
    companies = cursor.fetchall()
    conn.close()
    return companies
