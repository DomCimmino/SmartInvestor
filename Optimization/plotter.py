import yfinance as yf
import matplotlib.pyplot as plt


def plot_stock_history(tickers, top_3_companies):
    plt.figure(figsize=(12, 8))
    investment_results = []
    for ticker, (_, _, score) in zip(tickers, top_3_companies):
        stock_data = yf.Ticker(ticker).history(start="2019-01-01")
        start_price = stock_data["Close"].iloc[0]
        current_price = stock_data["Close"].iloc[-1]
        growth_percentage = ((current_price - start_price) / start_price) * 100
        initial_investment = 10000
        final_value = initial_investment * (current_price / start_price)
        investment_results.append((ticker, final_value))
        plt.plot(stock_data.index, stock_data["Close"], label=f"{ticker} ({growth_percentage:.2f}%)")

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
