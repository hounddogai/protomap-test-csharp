namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg notice is in its life cycle.</summary>
public enum ReorgNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A reorg notice. Its code is unique within a tenant.</summary>
public sealed class ReorgNotice(string code, string title, double weight, string team, string checksum, ReorgNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public string Team { get; set; } = team;
    public string Checksum { get; set; } = checksum;
    public ReorgNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a reorg notice in status created.</summary>
    public static ReorgNotice NewReorgNotice(string code, string title)
    {
        return new ReorgNotice(code, title, 0.0, "", "", ReorgNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgNoticeStatus next)
    {
        switch (Status)
        {
            case ReorgNoticeStatus.Created:
                switch (next)
                {
                    case ReorgNoticeStatus.Retired:
                        return true;
                }
                return false;
            case ReorgNoticeStatus.Validated:
                switch (next)
                {
                    case ReorgNoticeStatus.Published:
                    case ReorgNoticeStatus.Retired:
                        return true;
                }
                return false;
            case ReorgNoticeStatus.Published:
                return next == ReorgNoticeStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ReorgNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Weight;
    }

    public ReorgNotice Copy()
    {
        return new ReorgNotice(Code, Title, Weight, Team, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
