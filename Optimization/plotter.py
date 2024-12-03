import yfinance as yf
import matplotlib.pyplot as plt
from matplotlib.patches import Ellipse
from datetime import datetime

COVID_PERIOD = (datetime(2020, 3, 1), datetime(2020, 8, 1))
WAR_PERIOD = (datetime(2022, 2, 1), datetime(2022, 7, 1))


def get_growth_percentage(start_price, current_price):
    return ((current_price - start_price) / start_price) * 100


def calculate_final_investment(initial_investment, start_price, current_price):
    return initial_investment * (current_price / start_price)


def add_ellipse(ax, center, width_days, height, color, label):
    ellipse = Ellipse(
        xy=center, width=width_days, height=height,
        edgecolor=color, facecolor=color, alpha=0.2, label=label
    )
    ax.add_patch(ellipse)


def plot_stock_history(tickers, top_3_companies):
    plt.figure(figsize=(12, 8))
    investment_results = []
    ax = plt.gca()

    for ticker, (_, _, score) in zip(tickers, top_3_companies):
        stock_data = yf.Ticker(ticker).history(start="2010-01-01")
        stock_data_2019 = stock_data.loc["2019-01-01":]
        start_price_2019 = stock_data_2019["Close"].iloc[0]
        current_price = stock_data["Close"].iloc[-1]
        growth_percentage = get_growth_percentage(start_price_2019, current_price)
        final_value = calculate_final_investment(10000, start_price_2019, current_price)
        investment_results.append((ticker, final_value))
        plt.plot(stock_data.index, stock_data["Close"], label=f"{ticker} ({growth_percentage:.2f}%) from 2019")
        plt.scatter(datetime(2019, 1, 1), start_price_2019, color='red', zorder=5)

    ylim = ax.get_ylim()
    covid_center = (COVID_PERIOD[0] + (COVID_PERIOD[1] - COVID_PERIOD[0]) / 2, ylim[1] * 0.2)
    war_center = (WAR_PERIOD[0] + (WAR_PERIOD[1] - WAR_PERIOD[0]) / 2, ylim[1] * 0.5)
    add_ellipse(ax, covid_center, (COVID_PERIOD[1] - COVID_PERIOD[0]).days, ylim[1] * 0.5, 'blue', "COVID-19")
    add_ellipse(ax, war_center, (WAR_PERIOD[1] - WAR_PERIOD[0]).days, ylim[1] * 1.2, 'orange', "War in Ukraine")

    plt.scatter([], [], color='red', label="Start 01/01/2019")
    plt.title("Historical performance of the top three selected companies")
    plt.xlabel("Date")
    plt.ylabel("Closing Price")

    ranking_text = "Ranking:\n" + "\n".join([
        f"{i + 1}. {company[0]} (Score: {company[2]:.2f})"
        for i, company in enumerate(top_3_companies)
    ])
    investment_text = "Investment Results (10k€ on 01/01/2019):\n" + "\n".join([
        f"{i + 1}. {result[0]}: {result[1]:,.2f}€"
        for i, result in enumerate(investment_results)
    ])

    plt.gcf().text(0.02, 0.98, ranking_text, fontsize=10, verticalalignment='top',
                   bbox=dict(facecolor='white', alpha=0.8))
    plt.gcf().text(0.7, 0.98, investment_text, fontsize=10, verticalalignment='top',
                   bbox=dict(facecolor='white', alpha=0.8))
    plt.subplots_adjust(top=0.85)
    plt.legend()
    plt.show()
