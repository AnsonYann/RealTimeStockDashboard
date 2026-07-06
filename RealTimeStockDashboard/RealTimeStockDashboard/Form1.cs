using RealTimeStockDashboard.Models;
using RealTimeStockDashboard.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WatchListService;

namespace RealTimeStockDashboard
{
    public partial class Form1 : Form
    {
        private readonly StockApiServices _stockApiService = new StockApiServices();
        private readonly WatchlistService _watchlistService = new WatchlistService();
        private readonly CurencyService _currencyService = new CurencyService();

        private Stock _currentStock = null;

        // Tracks simulated 7-day realistic trailing price trend
        private List<double> _priceHistory = new List<double>();

        // UI Controls
        private TextBox txtSearch;
        private Label lblSymbol;
        private Label lblPrice;
        private Label lblChange;
        private DataGridView dgvCurrency;
        private DataGridView dgvWatch;
        private Label lblUpdated;
        private Button btnSearch;
        private Button btnAdd;
        private Button btnRemove;
        private Button btnRefresh;
        private Panel chartPanel;

        private ComboBox cbFromCurrency;
        private ComboBox cbToCurrency;

        public Form1()
        {
            InitializeComponent();

            // Ignores Windows 125%/150% scaling distortion bugs
            this.AutoScaleMode = AutoScaleMode.None;

            BuatDesign();
            WireUpEvents();
            LoadSavedWatchlist();
        }

        void BuatDesign()
        {
            this.Text = "Real-Time Stock Market Dashboard";

            // Resizes entire window larger to act as a natural layout zoom-out
            this.Size = new Size(1000, 920);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 244, 248);
            this.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            this.AutoScroll = true;

            Panel main = new Panel();
            main.Location = new Point(20, 20);
            main.Size = new Size(940, 830);
            main.BorderStyle = BorderStyle.FixedSingle;
            main.BackColor = Color.White;
            main.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(main);

            Label title = new Label();
            title.Text = "📈 Real-Time Stock Market Dashboard";
            title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            title.TextAlign = ContentAlignment.MiddleCenter;
            title.Location = new Point(0, 15);
            title.Size = new Size(940, 40);
            main.Controls.Add(title);

            // ================= SEARCH PANEL =================
            Panel searchPanel = new Panel();
            searchPanel.Location = new Point(25, 75);
            searchPanel.Size = new Size(890, 70);
            searchPanel.BorderStyle = BorderStyle.FixedSingle;
            main.Controls.Add(searchPanel);

            Label searchLabel = new Label();
            searchLabel.Text = "🔍 Enter Stock Symbol:";
            searchLabel.Location = new Point(20, 22);
            searchLabel.AutoSize = true;
            searchPanel.Controls.Add(searchLabel);

            // Padding and dimensions fixed to prevent hidden typed text
            txtSearch = new TextBox();
            txtSearch.Text = "AAPL";
            txtSearch.Font = new Font("Segoe UI", 12F);
            txtSearch.Location = new Point(260, 18);
            txtSearch.Size = new Size(200, 34);
            searchPanel.Controls.Add(txtSearch);

            btnSearch = new Button();
            btnSearch.Text = "Search Stock";
            btnSearch.Location = new Point(470, 16);
            btnSearch.Size = new Size(150, 36);
            btnSearch.BackColor = Color.LightSkyBlue;
            btnSearch.UseVisualStyleBackColor = false;
            searchPanel.Controls.Add(btnSearch);

            // ================= STOCK INFO PANEL =================
            Panel stockPanel = new Panel();
            stockPanel.Location = new Point(25, 165);
            stockPanel.Size = new Size(890, 190);
            stockPanel.BorderStyle = BorderStyle.FixedSingle;
            main.Controls.Add(stockPanel);

            lblSymbol = new Label();
            lblSymbol.Text = "🏷 Symbol: ---";
            lblSymbol.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lblSymbol.Location = new Point(30, 30);
            lblSymbol.AutoSize = true;
            stockPanel.Controls.Add(lblSymbol);

            lblPrice = new Label();
            lblPrice.Text = "💲 Price: ---";
            lblPrice.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lblPrice.Location = new Point(30, 80);
            lblPrice.AutoSize = true;
            stockPanel.Controls.Add(lblPrice);

            lblChange = new Label();
            lblChange.Text = "📊 Change: ---";
            lblChange.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lblChange.Location = new Point(30, 130);
            lblChange.AutoSize = true;
            stockPanel.Controls.Add(lblChange);

            Label chartTitle = new Label();
            chartTitle.Text = "📈 Real-Time Price Trend";
            chartTitle.Location = new Point(550, 15);
            chartTitle.AutoSize = true;
            stockPanel.Controls.Add(chartTitle);

            chartPanel = new Panel();
            chartPanel.Location = new Point(500, 45);
            chartPanel.Size = new Size(360, 120);
            chartPanel.BorderStyle = BorderStyle.FixedSingle;
            chartPanel.BackColor = Color.FromArgb(250, 252, 255);
            chartPanel.Paint += ChartPanel_Paint;
            stockPanel.Controls.Add(chartPanel);

            // ================= INTERACTIVE CURRENCY PANEL =================
            Panel currencyPanel = new Panel();
            currencyPanel.Location = new Point(25, 375);
            currencyPanel.Size = new Size(890, 230);
            currencyPanel.BorderStyle = BorderStyle.FixedSingle;
            main.Controls.Add(currencyPanel);

            Label currencyTitle = new Label();
            currencyTitle.Text = "💱 Currency Search";
            currencyTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            currencyTitle.Location = new Point(20, 15);
            currencyTitle.AutoSize = true;
            currencyPanel.Controls.Add(currencyTitle);

            Label lblFrom = new Label { Text = "Base:", Location = new Point(430, 18), AutoSize = true };
            cbFromCurrency = new ComboBox { Location = new Point(495, 14), Size = new Size(85, 33), DropDownStyle = ComboBoxStyle.DropDownList };
            cbFromCurrency.Items.AddRange(new string[] { "USD", "EUR", "GBP", "JPY", "IDR", "SGD", "AUD","TWD"});
            cbFromCurrency.SelectedIndex = 0;

            Label lblTo = new Label { Text = "Target:", Location = new Point(585, 18), AutoSize = true };
            cbToCurrency = new ComboBox { Location = new Point(660, 14), Size = new Size(85, 33), DropDownStyle = ComboBoxStyle.DropDownList };
            cbToCurrency.Items.AddRange(new string[] { "EUR", "USD", "IDR", "JPY", "GBP", "SGD", "AUD", "TWD"});
            cbToCurrency.SelectedIndex = 0;

            currencyPanel.Controls.AddRange(new Control[] { lblFrom, cbFromCurrency, lblTo, cbToCurrency });

            dgvCurrency = new DataGridView();
            dgvCurrency.Location = new Point(20, 60);
            dgvCurrency.Size = new Size(850, 110);
            dgvCurrency.ColumnCount = 4;
            dgvCurrency.Columns[0].Name = "From Base";
            dgvCurrency.Columns[1].Name = "To Target";
            dgvCurrency.Columns[2].Name = "Exchange Rate";
            dgvCurrency.Columns[3].Name = "Status";
            dgvCurrency.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCurrency.RowHeadersVisible = false;
            dgvCurrency.AllowUserToAddRows = false;
            currencyPanel.Controls.Add(dgvCurrency);

            btnRefresh = new Button();
            btnRefresh.Text = "Convert Currency";
            btnRefresh.Location = new Point(20, 185);
            btnRefresh.Size = new Size(180, 36);
            btnRefresh.BackColor = Color.LightGreen;
            btnRefresh.UseVisualStyleBackColor = false;
            currencyPanel.Controls.Add(btnRefresh);

            lblUpdated = new Label();
            lblUpdated.Text = "Last updated: Press convert to load live data.";
            lblUpdated.Location = new Point(220, 192);
            lblUpdated.AutoSize = true;
            currencyPanel.Controls.Add(lblUpdated);

            // ================= WATCHLIST PANEL =================
            Panel watchPanel = new Panel();
            watchPanel.Location = new Point(25, 625);
            watchPanel.Size = new Size(890, 180);
            watchPanel.BorderStyle = BorderStyle.FixedSingle;
            main.Controls.Add(watchPanel);

            Label watchTitle = new Label();
            watchTitle.Text = "⭐ Favorites Watchlist";
            watchTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            watchTitle.Location = new Point(20, 15);
            watchTitle.AutoSize = true;
            watchPanel.Controls.Add(watchTitle);

            btnAdd = new Button();
            btnAdd.Text = "➕ Current Stock";
            btnAdd.Location = new Point(360, 10);
            btnAdd.Size = new Size(280, 36);
            btnAdd.BackColor = Color.Gold;
            btnAdd.UseVisualStyleBackColor = false;
            watchPanel.Controls.Add(btnAdd);

            btnRemove = new Button();
            btnRemove.Text = "❌ Remove";
            btnRemove.Location = new Point(650, 10);
            btnRemove.Size = new Size(210, 36);
            btnRemove.UseVisualStyleBackColor = false;
            watchPanel.Controls.Add(btnRemove);

            dgvWatch = new DataGridView();
            dgvWatch.Location = new Point(20, 55);
            dgvWatch.Size = new Size(850, 110);
            dgvWatch.ColumnCount = 2;
            dgvWatch.Columns[0].Name = "Symbol";
            dgvWatch.Columns[1].Name = "Name";
            dgvWatch.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvWatch.RowHeadersVisible = false;
            dgvWatch.AllowUserToAddRows = false;
            dgvWatch.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            watchPanel.Controls.Add(dgvWatch);
        }

        private void WireUpEvents()
        {
            btnSearch.Click += async (s, e) =>
            {
                string query = txtSearch.Text.Trim().ToUpper();
                if (string.IsNullOrEmpty(query)) return;

                try
                {
                    btnSearch.Enabled = false;
                    var result = await _stockApiService.GetStockAsync(query);

                    _currentStock = new Stock
                    {
                        Symbol = result.symbol,
                        Price = (decimal)result.price,
                        Change = (decimal)result.change
                    };

                    lblSymbol.Text = $"🏷 Symbol: {_currentStock.Symbol}";
                    lblPrice.Text = $"💲 Price: ${_currentStock.Price:F2}";
                    lblChange.Text = $"📊 Change: {(_currentStock.Change >= 0 ? "+" : "")}{_currentStock.Change:F2}%";
                    lblChange.ForeColor = _currentStock.Change >= 0 ? Color.Green : Color.DarkRed;

                    // NEW REALISTIC TRAILING TREND SYSTEM
                    _priceHistory.Clear();
                    double basePrice = result.price;
                    double changePercent = result.change / 100.0;

                    Random rand = new Random();
                    for (int i = 5; i >= 1; i--)
                    {
                        double dailyFluctuation = (rand.NextDouble() * 0.04) - 0.02; // +/- 2% variance
                        double historicalPoint = basePrice * (1.0 - (changePercent * (i / 5.0)) + dailyFluctuation);
                        _priceHistory.Add(historicalPoint);
                    }
                    _priceHistory.Add(basePrice); // Final point is the active real price

                    chartPanel.Invalidate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Stock search error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnSearch.Enabled = true;
                }
            };

            btnAdd.Click += (s, e) =>
            {
                if (_currentStock == null)
                {
                    MessageBox.Show("Please search a valid stock first!", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var item = new WatchListService.WatchlistItem
                {
                    StockSymbol = _currentStock.Symbol,
                    StockName = "Favorite Stock Row"
                };

                bool success = _watchlistService.AddToWatchlist(item);

                if (success)
                {
                    MessageBox.Show($"{item.StockSymbol} added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadSavedWatchlist();
                }
                else
                {
                    MessageBox.Show($"{item.StockSymbol} is already on your watchlist.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            btnRemove.Click += (s, e) =>
            {
                if (dgvWatch.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a stock row from the Watchlist to remove.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string symbolToRemove = dgvWatch.SelectedRows[0].Cells["Symbol"].Value.ToString();
                bool success = _watchlistService.RemoveFromWatchlist(symbolToRemove);

                if (success)
                {
                    MessageBox.Show($"{symbolToRemove} removed from watchlist.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadSavedWatchlist();
                }
            };

            btnRefresh.Click += async (s, e) =>
            {
                string baseCurrency = cbFromCurrency.SelectedItem.ToString();
                string targetCurrency = cbToCurrency.SelectedItem.ToString();

                if (baseCurrency == targetCurrency)
                {
                    MessageBox.Show("Base and Target currency cannot be identical!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    btnRefresh.Enabled = false;
                    dgvCurrency.Rows.Clear();

                    lblUpdated.Text = "Fetching live conversion values...";
                    var data = await _currencyService.GetExchangeRateAsync(baseCurrency, targetCurrency);

                    dgvCurrency.Rows.Add(data.FromCurrency, data.ToCurrency, data.Rate.ToString("F4"), "Active Live Rate");
                    lblUpdated.Text = $"Last converted: {DateTime.Now.ToString("hh:mm:ss tt")}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Currency conversion failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnRefresh.Enabled = true;
                }
            };
        }

        private void ChartPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (_priceHistory.Count < 2)
            {
                using (Font font = new Font("Segoe UI", 9, FontStyle.Italic))
                {
                    e.Graphics.DrawString("Search multiple stocks to plot a trend graph...", font, Brushes.Gray, new PointF(15, 45));
                }
                return;
            }

            double minPrice = double.MaxValue;
            double maxPrice = double.MinValue;
            foreach (var price in _priceHistory)
            {
                if (price < minPrice) minPrice = price;
                if (price > maxPrice) maxPrice = price;
            }

            if (maxPrice == minPrice) { maxPrice += 1; minPrice -= 1; }

            PointF[] points = new PointF[_priceHistory.Count];
            float xStep = (float)chartPanel.Width / (_priceHistory.Count - 1);

            for (int i = 0; i < _priceHistory.Count; i++)
            {
                float x = i * xStep;
                float ratio = (float)((_priceHistory[i] - minPrice) / (maxPrice - minPrice));
                float y = chartPanel.Height - 15 - (ratio * (chartPanel.Height - 30));
                points[i] = new PointF(x, y);
            }

            using (Pen pen = new Pen(Color.MediumSeaGreen, 3))
            {
                e.Graphics.DrawLines(pen, points);
            }

            foreach (var pt in points)
            {
                e.Graphics.FillEllipse(Brushes.DodgerBlue, pt.X - 4, pt.Y - 4, 8, 8);
            }
        }

        private void LoadSavedWatchlist()
        {
            dgvWatch.Rows.Clear();
            List<WatchListService.WatchlistItem> items = _watchlistService.LoadWatchlist();
            foreach (var item in items)
            {
                dgvWatch.Rows.Add(item.StockSymbol, item.StockName);
            }
        }
    }
}