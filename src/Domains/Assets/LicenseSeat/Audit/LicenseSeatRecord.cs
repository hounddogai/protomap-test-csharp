namespace Acme.Workforce.Assets.LicenseSeat.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat record is in its life cycle.</summary>
public enum LicenseSeatRecordStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a license seat record and its current status.</summary>
public sealed class LicenseSeatRecord(string code, string name, double score, string phone, int batchSize, int windowDays, LicenseSeatRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public string Phone { get; set; } = phone;
    public int BatchSize { get; set; } = batchSize;
    public int WindowDays { get; set; } = windowDays;
    public LicenseSeatRecordStatus Status { get; set; } = status;

    /// <summary>Starts a license seat record in status requested.</summary>
    public static LicenseSeatRecord NewLicenseSeatRecord(string code, string name)
    {
        return new LicenseSeatRecord(code, name, 0.0, "", 0, 0, LicenseSeatRecordStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatRecordStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatRecordStatus next)
    {
        switch (Status)
        {
            case LicenseSeatRecordStatus.Requested:
                switch (next)
                {
                    case LicenseSeatRecordStatus.Granted:
                    case LicenseSeatRecordStatus.Revoked:
                        return true;
                }
                return false;
            case LicenseSeatRecordStatus.Granted:
                switch (next)
                {
                    case LicenseSeatRecordStatus.Expired:
                        return true;
                }
                return false;
            case LicenseSeatRecordStatus.Revoked:
                switch (next)
                {
                    case LicenseSeatRecordStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Score * (double)WindowDays, 2);
    }

    public LicenseSeatRecord Copy()
    {
        return new LicenseSeatRecord(Code, Name, Score, Phone, BatchSize, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
