using Blazorise.Charts;
using Microsoft.AspNetCore.Components;
using WeatherApp.Client.Services;
using WeatherApp.Shared.Entities;


namespace WeatherAppClient.Features.Dashboard.Machines
{
    public partial class Details : IDisposable
    {
        [Inject]
        public ISensorDataService SensorDataService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        INotifierService Notifier { get; set; } = default!;

        [Parameter]
        public string SensorId { get; set; } = string.Empty;

        public Sensor Sensor { get; set; } = new Sensor();

        public bool isAlreadyInitialised;

        public LineChart<ChartData> lineChart = default!;

        private static List<string> backgroundColors = new()
        {
            ChartColor.FromRgba(0, 0, 0, 0.5f),
            ChartColor.FromRgba(255, 0, 0, 0.5f),
            ChartColor.FromRgba(0, 128, 0, 0.5f),
            ChartColor.FromRgba(0, 0, 255, 0.5f),
            ChartColor.FromRgba(128, 0, 0, 0.5f),
            ChartColor.FromRgba(0, 128, 128, 0.5f),
            ChartColor.FromRgba(0, 0, 128, 0.5f),
            ChartColor.FromRgba(128, 0, 128, 0.5f),
            ChartColor.FromRgba(128, 128, 0, 0.5f),
            ChartColor.FromRgba(0, 255, 0, 0.5f),
            ChartColor.FromRgba(0, 255, 255, 0.5f)
        };
        private static List<string> borderColors = new()
        {
            ChartColor.FromRgba(0, 0, 0, 1),
            ChartColor.FromRgba(255, 0, 0, 1),
            ChartColor.FromRgba(0, 128, 0, 1),
            ChartColor.FromRgba(0, 0, 255, 1),
            ChartColor.FromRgba(128, 0, 0, 1),
            ChartColor.FromRgba(0, 128, 128, 1),
            ChartColor.FromRgba(0, 0, 128, 1),
            ChartColor.FromRgba(128, 0, 128, 1),
            ChartColor.FromRgba(128, 128, 0, 1),
            ChartColor.FromRgba(0, 255, 0, 1),
            ChartColor.FromRgba(0, 255, 255, 1)
        };
        private static List<string> textColors = new()
        {
            ChartColor.FromRgba(0, 0, 255, 1),
            ChartColor.FromRgba(0, 0, 0, 1)
        };

        private static List<string> xlabel = new() { "time of day" };
        private static List<string> ylabel = new() { "degrees °C" };

        public LineChartOptions LineChartOptions = new()
        {
            Parsing = new ChartParsing
            {
                XAxisKey = "time",
                YAxisKey = "data",
            },
            Plugins = new ChartPlugins
            {
                Legend = new ChartLegend
                {
                    Align = "end",
                    Labels = new ChartLegendLabel
                    {
                        Color = textColors[1],
                        BoxHeight = 20
                    }
                }
            },
            Scales = new ChartScales
            {
                X = new ChartAxis
                {
                    Title = new ChartScaleTitle
                    {
                        Display = true,
                        Text = xlabel[0],
                        Color = textColors[0]
                    },
                    Ticks = new ChartAxisTicks
                    {
                        Color = textColors[1]
                    }
                },
                Y = new ChartAxis
                {
                    Title = new ChartScaleTitle
                    {
                        Display = true,
                        Text = ylabel[0],
                        Color = textColors[0]
                    },
                    SuggestedMin = 0,
                    Ticks = new ChartAxisTicks
                    {
                        Color = textColors[1]
                    }
                }
            }
        };

        protected override async Task OnInitializedAsync()
        {
            Notifier.Notify += OnNotify;
            Sensor = await SensorDataService.GetSensorDetails(Guid.Parse(SensorId)) ?? new Sensor();
            await DrawChart();
        }

        // For page refresh
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!isAlreadyInitialised)
            {
                await DrawChart();
            }
        }

        private async Task DrawChart()
        {
            if (Sensor.SensorData.Count == 0) return;

            isAlreadyInitialised = true;
            await lineChart.Clear();

            var chartData = GetChartData();

            for (var i = 0; i < chartData.Count; i++)
            {
                // ignore datasets with less than 4 points of data
                if (chartData[i].Count < 4) continue;

                var label = chartData[i][0].ChartLabel;
                await lineChart.AddDataSet(GetLineChartDataset(chartData[i], i % backgroundColors.Count, i != 0, label));
            }
        }

        public async Task OnNotify(string key, Sensor machine)
        {
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            Notifier.Notify -= OnNotify;
        }

        public LineChartDataset<ChartData> GetLineChartDataset(List<ChartData> data, int colorIndex, bool hidden, string label)
        {
            return new LineChartDataset<ChartData>
            {
                Label = label,
                Data = data,
                BackgroundColor = backgroundColors[colorIndex],
                BorderColor = borderColors[colorIndex],
                Fill = false,
                PointRadius = 3,
                CubicInterpolationMode = "monotone",
                Hidden = hidden
            };
        }

        private List<List<ChartData>> GetChartData()
        {
            var dataSets = new List<List<ChartData>>();

            // If we have multiple readings for one hour,
            // group them together and calculate the average.
            var hourlyData = Sensor.SensorData
                .GroupBy(d => new
                {
                    d.Time.Date,
                    d.Time.Hour
                })
                .Select(grp => new
                {
                    Time = grp.Key.Date.Add(new TimeSpan(grp.Key.Hour, 0, 0)),
                    Data = grp.Average(g => g.Data)
                }).ToList();

            DateTime currentDate = DateTime.MinValue.Date;
            List<ChartData> currentSet = new List<ChartData>();


            if (hourlyData.Count > 0)
            {
                var first = hourlyData.First().Time;
                currentDate = first.Date;
                var timePadding = new DateTime();

                for (var hour = 0; hour < first.Hour; hour++, timePadding = timePadding.AddHours(1))
                {
                    currentSet.Insert(hour, new ChartData
                    {
                        Time = $"{timePadding.ToString("HH:mm")}",
                        Data = null,
                        ChartLabel = first.ToString("MMMM dd")
                    });
                }
            }

            // Create a daily dataset by grouping the hourly data by the day.
            foreach (var data in hourlyData)
            {
                var date = data.Time.Date;

                if (currentDate != date)
                {
                    dataSets.Add(currentSet);
                    currentSet = new List<ChartData>();
                    currentDate = date;
                }

                currentSet.Add(new ChartData
                {
                    Time = data.Time.ToString("HH:mm"),
                    Data = data.Data,
                    ChartLabel = data.Time.ToString("MMMM dd")
                });
            }

            dataSets.Add(currentSet);

            return dataSets;
        }

        public record ChartData
        {
            public double? Data { get; set; }
            public string Time { get; set; } = string.Empty;
            public string ChartLabel { get; set; } = string.Empty;
        }

        protected async Task GetNewData(Guid id)
        {
            var newData = await SensorDataService.GetNewSensorData(id);
            if (newData != null)
            {
                Sensor.SensorData = newData.ToList();
                StateHasChanged();

                await DrawChart();
            }
        }
    }
}