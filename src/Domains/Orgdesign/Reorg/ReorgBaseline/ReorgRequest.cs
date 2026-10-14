namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg request is in its life cycle.</summary>
public enum ReorgRequestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A reorg request. Its code is unique within a tenant.</summary>
public sealed class ReorgRequest(string code, string title, int batchSize, double ratio, double discount, string managerId, string source, ReorgRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public double Ratio { get; set; } = ratio;
    public double Discount { get; set; } = discount;
    public string ManagerId { get; set; } = managerId;
    public string Source { get; set; } = source;
    public ReorgRequestStatus Status { get; set; } = status;

    /// <summary>Starts a reorg request in status created.</summary>
    public static ReorgRequest NewReorgRequest(string code, string title)
    {
        return new ReorgRequest(code, title, 0, 0.0, 0.0, "", "", ReorgRequestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgRequestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgRequestStatus next)
    {
        switch (Status)
        {
            case ReorgRequestStatus.Created:
                switch (next)
                {
                    case ReorgRequestStatus.Validated:
                    case ReorgRequestStatus.Published:
                        return true;
                }
                return false;
            case ReorgRequestStatus.Validated:
                switch (next)
                {
                    case ReorgRequestStatus.Published:
                    case ReorgRequestStatus.Retired:
                        return true;
                }
                return false;
            case ReorgRequestStatus.Published:
                return next == ReorgRequestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ReorgRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetExposure()
    {
        return Math.Round(Discount + (double)BatchSize, 2);
    }

    public ReorgRequest Copy()
    {
        return new ReorgRequest(Code, Title, BatchSize, Ratio, Discount, ManagerId, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
