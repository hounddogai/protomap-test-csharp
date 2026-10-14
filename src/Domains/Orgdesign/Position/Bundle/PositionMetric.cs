namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position metric is in its life cycle.</summary>
public enum PositionMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A position metric as the position team keeps it.</summary>
public sealed class PositionMetric(string code, string label, int updatedAt, string department, string team, int retries, double floorValue, PositionMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Department { get; set; } = department;
    public string Team { get; set; } = team;
    public int Retries { get; set; } = retries;
    public double FloorValue { get; set; } = floorValue;
    public PositionMetricStatus Status { get; set; } = status;

    /// <summary>Starts a position metric in status created.</summary>
    public static PositionMetric NewPositionMetric(string code, string label)
    {
        return new PositionMetric(code, label, 0, "", "", 0, 0.0, PositionMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionMetricStatus next)
    {
        switch (Status)
        {
            case PositionMetricStatus.Created:
                return next == PositionMetricStatus.Validated || next == PositionMetricStatus.Published;
            case PositionMetricStatus.Validated:
                return next == PositionMetricStatus.Retired;
            case PositionMetricStatus.Published:
                return next == PositionMetricStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PositionMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Retries + FloorValue, 2);
    }

    public PositionMetric Copy()
    {
        return new PositionMetric(Code, Label, UpdatedAt, Department, Team, Retries, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
