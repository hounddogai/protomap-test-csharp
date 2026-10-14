namespace Acme.Workforce.Orgdesign.Position.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual position summary is in its life cycle.</summary>
public enum ManualPositionSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A manual position summary. Its code is unique within a tenant.</summary>
public sealed class ManualPositionSummary(string code, string title, double floorValue, double amount, bool active, bool approved, string contactEmail, string team, string salaryBand, double rate, ManualPositionSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public double Amount { get; set; } = amount;
    public bool Active { get; set; } = active;
    public bool Approved { get; set; } = approved;
    public string ContactEmail { get; set; } = contactEmail;
    public string Team { get; set; } = team;
    public string SalaryBand { get; set; } = salaryBand;
    public double Rate { get; set; } = rate;
    public ManualPositionSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a manual position summary in status new.</summary>
    public static ManualPositionSummary NewManualPositionSummary(string code, string title)
    {
        return new ManualPositionSummary(
            code,
            title,
            0.0,
            0.0,
            false,
            false,
            "",
            "",
            "",
            0.0,
            ManualPositionSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPositionSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPositionSummaryStatus next)
    {
        switch (Status)
        {
            case ManualPositionSummaryStatus.New:
                switch (next)
                {
                    case ManualPositionSummaryStatus.Done:
                    case ManualPositionSummaryStatus.Failed:
                        return true;
                }
                return false;
            case ManualPositionSummaryStatus.Queued:
                return next == ManualPositionSummaryStatus.Running;
            case ManualPositionSummaryStatus.Running:
                switch (next)
                {
                    case ManualPositionSummaryStatus.Done:
                        return true;
                }
                return false;
            case ManualPositionSummaryStatus.Done:
                return next == ManualPositionSummaryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ManualPositionSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual position summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetTotal()
    {
        return Math.Round(Amount * Rate, 2);
    }

    public ManualPositionSummary Copy()
    {
        return new ManualPositionSummary(
            Code,
            Title,
            FloorValue,
            Amount,
            Active,
            Approved,
            ContactEmail,
            Team,
            SalaryBand,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
