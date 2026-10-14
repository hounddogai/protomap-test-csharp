namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled asset return record is in its life cycle.</summary>
public enum ScheduledAssetReturnRecordStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a scheduled asset return record and its current status.</summary>
public sealed class ScheduledAssetReturnRecord(string code, string label, int version, string jobCode, string timezone, double ratio, string location, ScheduledAssetReturnRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public string JobCode { get; set; } = jobCode;
    public string Timezone { get; set; } = timezone;
    public double Ratio { get; set; } = ratio;
    public string Location { get; set; } = location;
    public ScheduledAssetReturnRecordStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled asset return record in status idle.</summary>
    public static ScheduledAssetReturnRecord NewScheduledAssetReturnRecord(string code, string label)
    {
        return new ScheduledAssetReturnRecord(
            code,
            label,
            0,
            "",
            "",
            0.0,
            "",
            ScheduledAssetReturnRecordStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAssetReturnRecordStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAssetReturnRecordStatus next)
    {
        switch (Status)
        {
            case ScheduledAssetReturnRecordStatus.Idle:
                return next == ScheduledAssetReturnRecordStatus.Ready;
            case ScheduledAssetReturnRecordStatus.Warming:
                switch (next)
                {
                    case ScheduledAssetReturnRecordStatus.Draining:
                    case ScheduledAssetReturnRecordStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledAssetReturnRecordStatus.Ready:
                switch (next)
                {
                    case ScheduledAssetReturnRecordStatus.Draining:
                        return true;
                }
                return false;
            case ScheduledAssetReturnRecordStatus.Draining:
                switch (next)
                {
                    case ScheduledAssetReturnRecordStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledAssetReturnRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled asset return record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled asset return record"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Ratio * (double)Version, 2);
    }

    public ScheduledAssetReturnRecord Copy()
    {
        return new ScheduledAssetReturnRecord(Code, Label, Version, JobCode, Timezone, Ratio, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
