namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop metric is in its life cycle.</summary>
public enum LaptopMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One laptop metric, with what the planners need to act on it.</summary>
public sealed class LaptopMetric(string code, string label, string department, double discount, int startsAt, string phone, string costCenter, string unit, string segment, int revision, string locale, LaptopMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public double Discount { get; set; } = discount;
    public int StartsAt { get; set; } = startsAt;
    public string Phone { get; set; } = phone;
    public string CostCenter { get; set; } = costCenter;
    public string Unit { get; set; } = unit;
    public string Segment { get; set; } = segment;
    public int Revision { get; set; } = revision;
    public string Locale { get; set; } = locale;
    public LaptopMetricStatus Status { get; set; } = status;

    /// <summary>Starts a laptop metric in status draft.</summary>
    public static LaptopMetric NewLaptopMetric(string code, string label)
    {
        return new LaptopMetric(code, label, "", 0.0, 0, "", "", "", "", 0, "", LaptopMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopMetricStatus next)
    {
        switch (Status)
        {
            case LaptopMetricStatus.Draft:
                switch (next)
                {
                    case LaptopMetricStatus.Pending:
                        return true;
                }
                return false;
            case LaptopMetricStatus.Pending:
                switch (next)
                {
                    case LaptopMetricStatus.Active:
                        return true;
                }
                return false;
            case LaptopMetricStatus.Active:
                return next == LaptopMetricStatus.Suspended || next == LaptopMetricStatus.Closed;
            case LaptopMetricStatus.Suspended:
                switch (next)
                {
                    case LaptopMetricStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Discount * (double)Revision, 2);
    }

    public LaptopMetric Copy()
    {
        return new LaptopMetric(
            Code,
            Label,
            Department,
            Discount,
            StartsAt,
            Phone,
            CostCenter,
            Unit,
            Segment,
            Revision,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
