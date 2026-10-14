namespace Acme.Workforce.Kit.Table;

public sealed record Table(string Title, List<string> Headers, List<List<string>> Rows)
{
    public static Table NewTable(string title, List<string> headers)
    {
        return new Table(title, headers, new List<List<string>>());
    }

    public void AddRow(List<string> cells)
    {
        if (cells.Count != Headers.Count)
        {
            throw new InvalidOperationException($"row has {cells.Count} cells, table {Title} has {Headers.Count} columns");
        }
        Rows.Add(cells);
    }

    public List<int> Widths()
    {
        var widths = new List<int>();
        foreach (var header in Headers)
        {
            widths.Add(header.Length);
        }
        foreach (var row in Rows)
        {
            for (var i = 0; i < row.Count; i++)
            {
                if (row[i].Length > widths[i])
                {
                    widths[i] = row[i].Length;
                }
            }
        }
        return widths;
    }

    public string Render()
    {
        var widths = Widths();
        var lines = new List<string> { Title };
        var head = new List<string>();
        for (var i = 0; i < Headers.Count; i++)
        {
            head.Add(Headers[i].PadRight(widths[i], ' '));
        }
        lines.Add(string.Join(" | ", head));
        foreach (var row in Rows)
        {
            var padded = new List<string>();
            for (var i = 0; i < row.Count; i++)
            {
                padded.Add(row[i].PadRight(widths[i], ' '));
            }
            lines.Add(string.Join(" | ", padded));
        }
        return string.Join("\n", lines);
    }

    public string Csv()
    {
        var lines = new List<string> { string.Join(",", Headers) };
        foreach (var row in Rows)
        {
            lines.Add(string.Join(",", row));
        }
        return string.Join("\n", lines);
    }
}
