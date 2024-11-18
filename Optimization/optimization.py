from pulp import LpMaximize, LpProblem, LpVariable, lpSum


def filter_valid_companies(companies):
    return [
        company for company in companies
        if all(value is not None and not isinstance(value, float) or not (value != value or abs(value) == float('inf'))
               for value in [company[3], company[4], company[5], company[6]])
    ]


def setup_optimization_problem(valid_companies):
    problem = LpProblem("Company_Optimization", LpMaximize)
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

    return problem, var_dict


def extract_selected_companies(valid_companies, var_dict):
    return [
        (company[1], company[2], (company[7] / 100) + company[8] + company[9])
        for company in valid_companies if var_dict[company[0]].value() == 1
    ]
