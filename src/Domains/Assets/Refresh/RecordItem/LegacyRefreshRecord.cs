namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy refresh record is in its life cycle.</summary>
public enum LegacyRefreshRecordStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One legacy refresh record, with what the planners need to act on it.</summary>
public sealed class LegacyRefreshRecord(string code, string label, string costCenter, string source, double rate, int graceDays, double rating, string contactEmail, LegacyRefreshRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public string Source { get; set; } = source;
    public double Rate { get; set; } = rate;
    public int GraceDays { get; set; } = graceDays;
    public double Rating { get; set; } = rating;
    public string ContactEmail { get; set; } = contactEmail;
    public LegacyRefreshRecordStatus Status { get; set; } = status;

    /// <summary>Starts a legacy refresh record in status new.</summary>
    public static LegacyRefreshRecord NewLegacyRefreshRecord(string code, string label)
    {
        return new LegacyRefreshRecord(code, label, "", "", 0.0, 0, 0.0, "", LegacyRefreshRecordStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyRefreshRecordStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyRefreshRecordStatus next)
    {
        switch (Status)
        {
            case LegacyRefreshRecordStatus.New:
                switch (next)
                {
                    case LegacyRefreshRecordStatus.Queued:
                        return true;
                }
                return false;
            case LegacyRefreshRecordStatus.Queued:
                return next == LegacyRefreshRecordStatus.Running || next == LegacyRefreshRecordStatus.Done;
            case LegacyRefreshRecordStatus.Running:
                return next == LegacyRefreshRecordStatus.Done || next == LegacyRefreshRecordStatus.Failed;
            case LegacyRefreshRecordStatus.Done:
                switch (next)
                {
                    case LegacyRefreshRecordStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyRefreshRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy refresh record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"legacy refresh record"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)GraceDays + Rate, 2);
    }

    public LegacyRefreshRecord Copy()
    {
        return new LegacyRefreshRecord(
            Code,
            Label,
            CostCenter,
            Source,
            Rate,
            GraceDays,
            Rating,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
