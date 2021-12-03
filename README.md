# SignalHelper
Repository to help visualise and label OHLC data with buy/sell positions.

![App running](https://github.com/kkartavenka/SignalHelper/blob/master/SignalHelper/Img/Screenshot.png)


## Walkthrough:

- All operations exists in the context menu of the main chart.

- The app uses MetaTrader CSV export of OHLCV.

- The visualization is via high-performance library: [ScottPlot](https://github.com/ScottPlot/ScottPlot)

- An additional chart can be attached of the same file format. [Currently is not working well since some days cannot be matched due to market closures and other reasons]

- Buy/sell positions are find by initial smoothing using savitzky golay filter with further linear regression [under the development].
