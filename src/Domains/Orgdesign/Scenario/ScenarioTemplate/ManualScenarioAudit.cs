namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual scenario audit is in its life cycle.</summary>
public enum ManualScenarioAuditStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A manual scenario audit. Its code is unique within a tenant.</summary>
public sealed class ManualScenarioAudit(string code, string title, string locale, double amount, int capacity, bool visible, string location, ManualScenarioAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public double Amount { get; set; } = amount;
    public int Capacity { get; set; } = capacity;
    public bool Visible { get; set; } = visible;
    public string Location { get; set; } = location;
    public ManualScenarioAuditStatus Status { get; set; } = status;

    /// <summary>Starts a manual scenario audit in status new.</summary>
    public static ManualScenarioAudit NewManualScenarioAudit(string code, string title)
    {
        return new ManualScenarioAudit(code, title, "", 0.0, 0, false, "", ManualScenarioAuditStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualScenarioAuditStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualScenarioAuditStatus next)
    {
        switch (Status)
        {
            case ManualScenarioAuditStatus.New:
                return next == ManualScenarioAuditStatus.Running || next == ManualScenarioAuditStatus.Done;
            case ManualScenarioAuditStatus.Queued:
                return next == ManualScenarioAuditStatus.Running || next == ManualScenarioAuditStatus.Failed;
            case ManualScenarioAuditStatus.Running:
                switch (next)
                {
                    case ManualScenarioAuditStatus.Done:
                    case ManualScenarioAuditStatus.Failed:
                        return true;
                }
                return false;
            case ManualScenarioAuditStatus.Done:
                switch (next)
                {
                    case ManualScenarioAuditStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualScenarioAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual scenario audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Amount + (double)Capacity, 2);
    }

    public ManualScenarioAudit Copy()
    {
        return new ManualScenarioAudit(Code, Title, Locale, Amount, Capacity, Visible, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
