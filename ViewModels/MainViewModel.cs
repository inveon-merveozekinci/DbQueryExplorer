using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DbQueryExplorer.Models;
using DbQueryExplorer.Services;
using Microsoft.Win32;

namespace DbQueryExplorer.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // ──── Connection profiles (loaded from connections.json) ─────────────────
    public ObservableCollection<ConnectionProfile> Profiles { get; } = new();

    [ObservableProperty] private ConnectionProfile? _selectedProfile;
    [ObservableProperty] private bool _isManualMode;
    [ObservableProperty] private bool _hasProfiles;

    // ──── Manual connection fields ───────────────────────────────────────────
    [ObservableProperty] private string _server = "localhost";
    [ObservableProperty] private int _port = 3306;
    [ObservableProperty] private string _database = string.Empty;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private DatabaseType _selectedDbType = DatabaseType.MySQL;

    // ──── Save-to-profile ─────────────────────────────────────────────────────
    [ObservableProperty] private string _newProfileName = string.Empty;

    // ──── Custom SQL ──────────────────────────────────────────────────────────
    [ObservableProperty] private string _customSql = string.Empty;

    // ──── State ───────────────────────────────────────────────────────────────
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _connectionStatus = "Bağlantı yok";

    // ──── Tables ──────────────────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<string> _tables = new();
    [ObservableProperty] private string? _selectedTable;

    // ──── Filters ─────────────────────────────────────────────────────────────
    public ObservableCollection<FilterRow> Filters { get; } = new();

    // ──── Column selection ────────────────────────────────────────────────────
    public ObservableCollection<SelectableColumn> SelectableColumns { get; } = new();
    [ObservableProperty] private bool _hasColumns;

    // ──── JOIN / ValueMapper / extras ─────────────────────────────────────────
    [ObservableProperty] private string _tableAlias              = string.Empty;
    [ObservableProperty] private string _additionalWhere         = string.Empty;
    [ObservableProperty] private bool   _onlyRegisteredCustomers;
    [ObservableProperty] private bool   _includeAddressInfo;
    public ObservableCollection<JoinDefinition> Joins        { get; } = new();
    public ObservableCollection<ValueMapper>    ValueMappers { get; } = new();

    // ──── Results ─────────────────────────────────────────────────────────────
    [ObservableProperty] private DataView? _results;
    [ObservableProperty] private int _resultCount;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _hasResults;

    // ──── Row limit ───────────────────────────────────────────────────────────
    // 0 = no limit (fetch all). Default 5 000 protects against 800K-row tables.
    [ObservableProperty] private int _rowLimit = 5000;
    public int[] RowLimitOptions { get; } = { 1000, 5000, 10000, 50000, 0 };

    public IReadOnlyList<DatabaseType> DbTypes { get; } = Enum.GetValues<DatabaseType>();

    private IDbService? _dbService;
    private List<ColumnInfo> _currentColumns = new();
    private DatabaseType _connectedDbType;

    public MainViewModel()
    {
        LoadProfiles();
    }

    // ──── Load profiles from connections.json ─────────────────────────────────
    private void LoadProfiles()
    {
        var list = ConnectionLoader.Load();
        foreach (var p in list) Profiles.Add(p);

        HasProfiles = Profiles.Count > 0;
        IsManualMode = !HasProfiles;

        if (HasProfiles)
            SelectedProfile = Profiles[0];

        StatusMessage = HasProfiles
            ? $"{Profiles.Count} bağlantı profili yüklendi."
            : $"Profil bulunamadı — manuel bağlantı modu. ({ConnectionLoader.ConfigFilePath})";
    }

    // ──── Save current manual connection as a profile ────────────────────────
    [RelayCommand]
    private void SaveProfile()
    {
        var name = NewProfileName.Trim();
        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show("Lütfen bir profil adı girin.", "Uyarı",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var profile = new ConnectionProfile
        {
            Name     = name,
            DbType   = SelectedDbType == DatabaseType.MSSQL ? "MSSQL" : "MySQL",
            Server   = Server,
            Port     = Port,
            Database = Database,
            Username = Username,
            Password = Password
        };

        try
        {
            ConnectionLoader.SaveProfile(profile);

            if (!Profiles.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
                Profiles.Add(profile);
            else
            {
                var idx = Profiles.IndexOf(Profiles.First(p =>
                    string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)));
                Profiles[idx] = profile;
            }

            HasProfiles = true;
            StatusMessage = $"✓ '{name}' profili connections.json dosyasına kaydedildi.";
            NewProfileName = string.Empty;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kaydedilemedi: {ex.Message}", "Hata",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ──── Switch to manual mode ───────────────────────────────────────────────
    [RelayCommand]
    private void ToggleManualMode()
    {
        IsManualMode = !IsManualMode;
    }

    // ──── DB type changed → update default port ───────────────────────────────
    partial void OnSelectedDbTypeChanged(DatabaseType value)
        => Port = value == DatabaseType.MySQL ? 3306 : 1433;

    // ──── Connect ─────────────────────────────────────────────────────────────
    [RelayCommand]
    private async Task ConnectAsync()
    {
        IsBusy = true;
        ConnectionStatus = "Bağlanıyor...";
        try
        {
            _dbService?.Dispose();

            ConnectionInfo info;
            if (!IsManualMode && SelectedProfile is not null)
            {
                info = SelectedProfile.ToConnectionInfo();
            }
            else
            {
                info = new ConnectionInfo
                {
                    DbType   = SelectedDbType,
                    Server   = Server,
                    Port     = Port,
                    Database = Database,
                    Username = Username,
                    Password = Password
                };
            }

            _dbService = DbServiceFactory.Create(info);
            _connectedDbType = info.DbType;
            await _dbService.ConnectAsync();

            IsConnected = true;
            var label = (!IsManualMode && SelectedProfile is not null)
                            ? SelectedProfile.Name
                            : $"{Server}/{Database}";
            ConnectionStatus = $"✓ Bağlandı → {label}";

            await LoadTablesAsync();
        }
        catch (Exception ex)
        {
            IsConnected = false;
            ConnectionStatus = $"✗ Hata: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ──── Load tables ─────────────────────────────────────────────────────────
    private async Task LoadTablesAsync()
    {
        var list = await _dbService!.GetTablesAsync();
        Tables.Clear();
        foreach (var t in list) Tables.Add(t);
    }

    // ──── Table selected ──────────────────────────────────────────────────────
    partial void OnSelectedTableChanged(string? value)
    {
        if (value is null) { TableAlias = string.Empty; return; }
        TableAlias = value[0].ToString().ToLower();
        _ = LoadColumnsAsync(value);
    }

    private async Task LoadColumnsAsync(string tableName)
    {
        IsBusy = true;
        try
        {
            _currentColumns = await _dbService!.GetColumnsAsync(tableName);

            SelectableColumns.Clear();
            foreach (var col in _currentColumns)
                SelectableColumns.Add(new SelectableColumn { Name = col.Name, DataType = col.DataType, IsSelected = true });
            HasColumns = SelectableColumns.Count > 0;

            Filters.Clear();
            Results = null;
            HasResults = false;
            ResultCount = 0;
            StatusMessage = $"{tableName} seçildi — {_currentColumns.Count} sütun. Filtre ekleyip sorgu çalıştırın.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Sütunlar yüklenemedi: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ──── Run custom SQL query ────────────────────────────────────────────────
    [RelayCommand]
    private async Task RunCustomQueryAsync()
    {
        if (_dbService is null)
        {
            StatusMessage = "Lütfen önce bağlanın.";
            return;
        }
        if (string.IsNullOrWhiteSpace(CustomSql))
        {
            StatusMessage = "SQL sorgusu boş.";
            return;
        }

        IsBusy = true;
        StatusMessage = "SQL sorgusu çalıştırılıyor...";
        try
        {
            var dt = await _dbService.ExecuteRawSqlAsync(CustomSql);
            Results     = dt.DefaultView;
            ResultCount = dt.Rows.Count;
            HasResults  = dt.Rows.Count > 0;
            StatusMessage = $"{ResultCount} kayıt bulundu.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Sorgu hatası: {ex.Message}";
            MessageBox.Show(ex.Message, "SQL Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ──── JOIN commands ───────────────────────────────────────────────────────
    [RelayCommand]
    private void AddJoin()
    {
        Joins.Add(new JoinDefinition
        {
            AvailableColumns = new ObservableCollection<string>(_currentColumns.Select(c => c.Name))
        });
    }

    [RelayCommand]
    private void RemoveJoin(JoinDefinition join) => Joins.Remove(join);

    [RelayCommand]
    private void AddJoinCondition(JoinDefinition join)
        => join.ExtraConditions.Add(new JoinExtraCondition());

    [RelayCommand]
    private void RemoveJoinCondition(JoinExtraCondition cond)
    {
        foreach (var j in Joins)
            if (j.ExtraConditions.Remove(cond)) break;
    }

    // ──── ValueMapper commands ────────────────────────────────────────────────
    [RelayCommand]
    private void AddValueMapper()
    {
        var mapper = new ValueMapper
        {
            AvailableColumns = new ObservableCollection<string>(_currentColumns.Select(c => c.Name))
        };
        mapper.Mappings.Add(new ValueMapping());
        ValueMappers.Add(mapper);
    }

    [RelayCommand]
    private void RemoveValueMapper(ValueMapper mapper) => ValueMappers.Remove(mapper);

    [RelayCommand]
    private void AddValueMapping(ValueMapper mapper)
        => mapper.Mappings.Add(new ValueMapping());

    [RelayCommand]
    private void RemoveValueMapping(ValueMapping mapping)
    {
        foreach (var m in ValueMappers)
            if (m.Mappings.Remove(mapping)) break;
    }

    // ──── Column selection ────────────────────────────────────────────────────
    [RelayCommand]
    private void SelectAllColumns()
    {
        foreach (var col in SelectableColumns) col.IsSelected = true;
    }

    [RelayCommand]
    private void DeselectAllColumns()
    {
        foreach (var col in SelectableColumns) col.IsSelected = false;
    }

    // ──── Filters ─────────────────────────────────────────────────────────────
    [RelayCommand]
    private void AddFilter()
    {
        var cols = new ObservableCollection<string>(_currentColumns.Select(c => c.Name));
        Filters.Add(new FilterRow
        {
            AvailableColumns = cols,
            ColumnName       = cols.FirstOrDefault() ?? string.Empty
        });
    }

    [RelayCommand]
    private void RemoveFilter(FilterRow filter) => Filters.Remove(filter);

    [RelayCommand]
    private void ClearFilters() => Filters.Clear();

    // ──── Run query (table mode) ──────────────────────────────────────────────
    [RelayCommand]
    private async Task RunQueryAsync()
    {
        if (_dbService is null || SelectedTable is null) return;

        IsBusy = true;
        StatusMessage = "Sorgu çalıştırılıyor...";
        try
        {
            var (sql, parameters) = BuildTableModeQuery();
            var dt = await _dbService.ExecuteRawSqlAsync(sql, parameters);
            Results     = dt.DefaultView;
            ResultCount = dt.Rows.Count;
            HasResults  = dt.Rows.Count > 0;
            StatusMessage = (RowLimit > 0 && ResultCount == RowLimit)
                ? $"İlk {RowLimit} kayıt gösteriliyor (limit aktif). Tümünü görmek için soldan \"Tümü\" seçin."
                : $"{ResultCount} kayıt bulundu.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Sorgu hatası: {ex.Message}";
            MessageBox.Show(ex.Message, "Sorgu Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ──── Build SQL from table mode UI ────────────────────────────────────────
    private (string Sql, Dictionary<string, object> Parameters) BuildTableModeQuery()
    {
        var isMssql = _connectedDbType == DatabaseType.MSSQL;
        string Q(string n) => isMssql ? $"[{n}]" : $"`{n}`";
        var nolock = isMssql ? " WITH (NOLOCK)" : "";

        var alias  = TableAlias.Trim();
        var prefix = string.IsNullOrEmpty(alias) ? "" : $"{alias}.";

        // ── SELECT ────────────────────────────────────────────────────────────
        var colParts = new List<string>();
        var selected = SelectableColumns.Where(c => c.IsSelected).ToList();
        var hasMappers = ValueMappers.Any(m => m.IsEnabled && !string.IsNullOrWhiteSpace(m.SourceColumn));

        // Columns replaced by a ValueMapper — skip raw column to avoid duplicates
        var mappedColNames = new HashSet<string>(
            ValueMappers
                .Where(m => m.IsEnabled && !string.IsNullOrWhiteSpace(m.SourceColumn))
                .Select(m => m.SourceColumn.Split('.').Last()),
            StringComparer.OrdinalIgnoreCase);

        if (selected.Count == SelectableColumns.Count && !hasMappers)
            colParts.Add(string.IsNullOrEmpty(alias) ? "*" : $"{alias}.*");
        else
            colParts.AddRange(selected
                .Where(c => !mappedColNames.Contains(c.Name))
                .Select(c => $"{prefix}{Q(c.Name)}"));

        foreach (var mapper in ValueMappers.Where(m => m.IsEnabled && !string.IsNullOrWhiteSpace(m.SourceColumn)))
        {
            var srcCol = mapper.SourceColumn.Contains('.') ? mapper.SourceColumn : $"{prefix}{Q(mapper.SourceColumn)}";
            var caseExpr = new StringBuilder("CASE");
            foreach (var m in mapper.Mappings.Where(m => !string.IsNullOrWhiteSpace(m.SourceValue)))
                caseExpr.Append($" WHEN {srcCol} = '{m.SourceValue}' THEN '{m.DisplayValue}'");
            var defVal = string.IsNullOrWhiteSpace(mapper.DefaultValue) ? "NULL" : $"'{mapper.DefaultValue}'";
            caseExpr.Append($" ELSE {defVal} END");
            var outName = string.IsNullOrWhiteSpace(mapper.OutputAlias) ? mapper.SourceColumn.Split('.').Last() : mapper.OutputAlias;
            caseExpr.Append($" AS {Q(outName)}");
            colParts.Add(caseExpr.ToString());
        }

        // ── Address info COALESCE columns ─────────────────────────────────────
        if (IncludeAddressInfo)
        {
            colParts.Add($"COALESCE(ab.{Q("TaxNumber")}, ashp.{Q("TaxNumber")}) AS {Q("TaxNumber")}");
            colParts.Add($"COALESCE(ab.{Q("TaxOffice")}, ashp.{Q("TaxOffice")}) AS {Q("TaxOffice")}");
        }

        var sb = new StringBuilder("SELECT ");
        // TOP N: MSSQL syntax; 0 = no limit
        if (isMssql && RowLimit > 0)
            sb.Append($"TOP {RowLimit} ");
        sb.Append(string.Join(",\n       ", colParts));

        // ── FROM ──────────────────────────────────────────────────────────────
        var tableId = string.IsNullOrEmpty(alias) ? Q(SelectedTable!) : $"{Q(SelectedTable!)} {alias}";
        sb.Append($"\nFROM {tableId}{nolock}");

        // ── Registered customers JOIN ─────────────────────────────────────────
        if (OnlyRegisteredCustomers)
        {
            sb.Append($"\nINNER JOIN {Q("Customer_CustomerRole_Mapping")} crm{nolock}");
            sb.Append($"\n    ON crm.{Q("Customer_Id")} = {prefix}{Q("Id")}");
            sb.Append($"\n    AND crm.{Q("CustomerRole_Id")} = 3");
        }

        // ── Address JOINs ─────────────────────────────────────────────────────
        if (IncludeAddressInfo)
        {
            sb.Append($"\nLEFT JOIN {Q("Address")} ab{nolock}   ON ab.{Q("Id")} = {prefix}{Q("BillingAddress_Id")}");
            sb.Append($"\nLEFT JOIN {Q("Address")} ashp{nolock} ON ashp.{Q("Id")} = {prefix}{Q("ShippingAddress_Id")}");
        }

        // ── WHERE ─────────────────────────────────────────────────────────────
        var conditions = new List<string>();
        var parameters = new Dictionary<string, object>();
        int idx = 0;

        foreach (var f in Filters.Where(f => f.IsEnabled && !string.IsNullOrWhiteSpace(f.ColumnName)))
        {
            var colName = f.ColumnName;
            var col     = colName.Contains('.') ? colName : $"{prefix}{Q(colName)}";
            var op      = f.SelectedOperator;

            if (op is "IS NULL")     { conditions.Add($"{col} IS NULL"); continue; }
            if (op is "IS NOT NULL") { conditions.Add($"{col} IS NOT NULL"); continue; }
            if (op is "IN")
            {
                var parts  = (f.Value ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
                var pnames = parts.Select((_, i) => $"@p{idx + i}").ToList();
                for (int i = 0; i < parts.Length; i++) parameters[$"@p{idx + i}"] = parts[i].Trim();
                idx += parts.Length;
                conditions.Add($"{col} IN ({string.Join(", ", pnames)})");
                continue;
            }
            var pname = $"@p{idx++}";
            object val = f.Value ?? "";
            if (op is "LIKE" or "NOT LIKE") val = $"%{val}%";
            parameters[pname] = val;
            conditions.Add($"{col} {op} {pname}");
        }

        if (!string.IsNullOrWhiteSpace(AdditionalWhere))
            conditions.Add($"({AdditionalWhere.Trim()})");

        if (conditions.Count > 0)
            sb.Append("\nWHERE " + string.Join("\n  AND ", conditions));

        return (sb.ToString(), parameters);
    }

    // ──── Export Excel ────────────────────────────────────────────────────────
    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        if (Results is null || Results.Count == 0)
        {
            MessageBox.Show("Dışa aktarılacak veri yok.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dlg = new SaveFileDialog
        {
            Filter   = "Excel Dosyası (*.xlsx)|*.xlsx",
            FileName = $"{SelectedTable ?? "sorgu"}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };

        if (dlg.ShowDialog() != true) return;

        // Take a snapshot of the DataTable on the UI thread before going async
        var table    = Results.Table!.Copy();
        var fileName = dlg.FileName;

        try
        {
            IsBusy = true;
            StatusMessage = "Excel dosyası oluşturuluyor...";

            // Run the CPU-intensive export on a background thread — UI stays responsive
            await Task.Run(() => ExcelExportService.Export(table, fileName));

            StatusMessage = $"✓ Dosya kaydedildi: {fileName}";

            if (MessageBox.Show("Dosya kaydedildi. Açmak ister misiniz?", "Başarılı",
                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(fileName) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Excel oluşturulurken hata: {ex.Message}", "Hata",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ──── Disconnect ──────────────────────────────────────────────────────────
    [RelayCommand]
    private void Disconnect()
    {
        _dbService?.Dispose();
        _dbService = null;
        IsConnected = false;
        Tables.Clear();
        Filters.Clear();
        Results    = null;
        HasResults = false;
        SelectableColumns.Clear();
        HasColumns = false;
        Joins.Clear();
        ValueMappers.Clear();
        TableAlias               = string.Empty;
        AdditionalWhere          = string.Empty;
        OnlyRegisteredCustomers  = false;
        IncludeAddressInfo       = false;
        ConnectionStatus = "Bağlantı yok";
        StatusMessage    = string.Empty;
    }
}
