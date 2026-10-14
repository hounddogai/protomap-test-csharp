namespace Acme.Workforce.Orgdesign.CostModel.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model audit is in its life cycle.</summary>
public enum CostModelAuditStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a cost model audit and its current status.</summary>
public sealed class CostModelAudit(string code, string title, string location, string source, string region, int createdAt, CostModelAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public string Source { get; set; } = source;
    public string Region { get; set; } = region;
    public int CreatedAt { get; set; } = createdAt;
    public CostModelAuditStatus Status { get; set; } = status;

    /// <summary>Starts a cost model audit in status requested.</summary>
    public static CostModelAudit NewCostModelAudit(string code, string title)
    {
        return new CostModelAudit(code, title, "", "", "", 0, CostModelAuditStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelAuditStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelAuditStatus next)
    {
        switch (Status)
        {
            case CostModelAuditStatus.Requested:
                switch (next)
                {
                    case CostModelAuditStatus.Revoked:
                        return true;
                }
                return false;
            case CostModelAuditStatus.Granted:
                switch (next)
                {
                    case CostModelAuditStatus.Revoked:
                    case CostModelAuditStatus.Expired:
                        return true;
                }
                return false;
            case CostModelAuditStatus.Revoked:
                return next == CostModelAuditStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CostModelAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"cost model audit"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public CostModelAudit Copy()
    {
        return new CostModelAudit(Code, Title, Location, Source, Region, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
