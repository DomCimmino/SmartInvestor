import yfinance as yf
import matplotlib.pyplot as plt
from matplotlib.patches import Ellipse
from datetime import datetime

# Constants
COVID_PERIOD = (datetime(2020, 1, 1), datetime(2020, 8, 1))
UKRAINE_WAR_PERIOD = (datetime(2022, 2, 1), datetime(2022, 7, 1))
GAZA_WAR_PERIOD = (datetime(2023, 8, 1), datetime(2024, 1, 1))
INITIAL_INVESTMENT = 10000
START_DATE = "2010-01-01"
START_ANALYSIS_DATE = "2019-01-01"
SP500_TICKER = "^GSPC"


def calculate_growth_percentage(start_price, current_price):
    return ((current_price - start_price) / start_price) * 100


def calculate_final_investment(start_price, current_price):
    return INITIAL_INVESTMENT * (current_price / start_price)


def add_highlight_ellipse(ax, period, center_y, height, color, label):
    width_days = (period[1] - period[0]).days
    center_x = period[0] + (period[1] - period[0]) / 2
    ellipse = Ellipse(
        xy=(center_x, center_y), width=width_days, height=height,
        edgecolor=color, facecolor=color, alpha=0.2, label=label
    )
    ax.add_patch(ellipse)


def get_stock_data(ticker):
    stock = yf.Ticker(ticker)
    return stock.history(start=START_DATE)


def process_investment_data(stock_data, start_analysis_date):
    stock_data_2019 = stock_data.loc[start_analysis_date:]
    start_price = stock_data_2019["Close"].iloc[0]
    current_price = stock_data["Close"].iloc[-1]
    return start_price, current_price


def plot_stock_history(fig, ax, tickers, top_companies):
    investment_results = []
    for ticker, (_, _, score) in zip(tickers, top_companies):
        stock_data = get_stock_data(ticker)
        start_price_2019, current_price = process_investment_data(stock_data, START_ANALYSIS_DATE)

        growth_percentage = calculate_growth_percentage(start_price_2019, current_price)
        final_value = calculate_final_investment(start_price_2019, current_price)

        investment_results.append((ticker, final_value))
        ax.plot(stock_data.index, stock_data["Close"], label=f"{ticker} ({growth_percentage:.2f}%) from 2019")
        ax.scatter(datetime(2019, 1, 1), start_price_2019, color='red', zorder=5)

    ylim = ax.get_ylim()
    add_highlight_ellipse(ax, COVID_PERIOD, ylim[1] * 0.35, ylim[1] * 0.8, 'blue', "COVID-19")
    add_highlight_ellipse(ax, UKRAINE_WAR_PERIOD, ylim[1] * 0.5, ylim[1] * 1.2, 'orange', "War in Ukraine")
    add_highlight_ellipse(ax, GAZA_WAR_PERIOD, ylim[1] * 0.5, ylim[1] * 1.2, 'brown', "War in Gaza")

    ax.scatter([], [], color='red', label="Start Investment (01/01/2019)")
    ax.set_title("Historical Performance of Top Companies")
    ax.set_xlabel("Date")
    ax.set_ylabel("Closing Price")
    ax.legend()

    ranking_text = "\n".join(
        [f"{i + 1}. {company[0]} (Score: {company[2]:.2f})" for i, company in enumerate(top_companies)])
    investment_text = "\n".join(
        [f"{i + 1}. {result[0]}: {result[1]:,.2f}€" for i, result in enumerate(investment_results)])

    fig.text(0.038, 0.98, f"Ranking:\n{ranking_text}", fontsize=10, verticalalignment='top',
             bbox=dict(facecolor='white', alpha=0.8))
    fig.text(0.344, 0.98, f"Investment Results (10k€ on 01/01/2019):\n{investment_text}", fontsize=10,
             verticalalignment='top', bbox=dict(facecolor='white', alpha=0.8))


def plot_sp500_performance(fig, ax):
    sp500_data = get_stock_data(SP500_TICKER)
    sp500_start_price, sp500_current_price = process_investment_data(sp500_data, START_ANALYSIS_DATE)
    sp500_growth = calculate_growth_percentage(sp500_start_price, sp500_current_price)

    ax.plot(sp500_data.index, sp500_data["Close"], label=f"S&P 500 ({sp500_growth:.2f}%) from 2019", color='blue')
    ax.scatter(datetime(2019, 1, 1), sp500_start_price, color='red', label="Start Investment (01/01/2019)", zorder=5)

    ylim = ax.get_ylim()
    add_highlight_ellipse(ax, COVID_PERIOD, ylim[1] * 0.5, ylim[1] * 0.3, 'blue', "COVID-19")
    add_highlight_ellipse(ax, UKRAINE_WAR_PERIOD, ylim[1] * 0.68, ylim[1] * 0.3, 'orange', "War in Ukraine")
    add_highlight_ellipse(ax, GAZA_WAR_PERIOD, ylim[1] * 0.7, ylim[1] * 0.2, 'brown', "War in Gaza")

    ax.set_title("S&P 500 Historical Performance")
    ax.set_xlabel("Date")
    ax.set_ylabel("Closing Price")
    ax.legend()

    investment_result = calculate_final_investment(sp500_start_price, sp500_current_price)
    fig.text(0.843, 0.955, f"Investment Results (10k€ on 01/01/2019):\n{investment_result:,.2f}€",
             fontsize=10, verticalalignment='top', bbox=dict(facecolor='white', alpha=0.8))


def plot(tickers, top_companies):
    fig, axes = plt.subplots(1, 2, figsize=(20, 12))

    plot_stock_history(fig, axes[0], tickers, top_companies)
    plot_sp500_performance(fig, axes[1])

    fig.tight_layout(rect=[0, 0, 1, 0.95])
    fig.subplots_adjust(top=0.90)
    plt.show()
