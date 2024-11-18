from db_handler import fetch_companies
from optimization import filter_valid_companies, setup_optimization_problem, extract_selected_companies
from plotter import plot_stock_history
from operator import itemgetter


def main():
    companies = fetch_companies()
    valid_companies = filter_valid_companies(companies)
    problem, var_dict = setup_optimization_problem(valid_companies)

    problem.solve()

    selected_companies = extract_selected_companies(valid_companies, var_dict)
    selected_companies.sort(key=itemgetter(2), reverse=True)

    top_3_companies = selected_companies[:3]
    top_3_tickers = [company[1] for company in top_3_companies]
    plot_stock_history(top_3_tickers, top_3_companies)


if __name__ == "__main__":
    main()
