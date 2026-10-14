namespace Acme.Workforce.Assets.Laptop.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop template is in its life cycle.</summary>
public enum LaptopTemplateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One laptop template, with what the planners need to act on it.</summary>
public sealed class LaptopTemplate(string code, string title, string grade, string department, string unit, double score, LaptopTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public string Department { get; set; } = department;
    public string Unit { get; set; } = unit;
    public double Score { get; set; } = score;
    public LaptopTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a laptop template in status new.</summary>
    public static LaptopTemplate NewLaptopTemplate(string code, string title)
    {
        return new LaptopTemplate(code, title, "", "", "", 0.0, LaptopTemplateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopTemplateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopTemplateStatus next)
    {
        switch (Status)
        {
            case LaptopTemplateStatus.New:
                switch (next)
                {
                    case LaptopTemplateStatus.Running:
                        return true;
                }
                return false;
            case LaptopTemplateStatus.Queued:
                return next == LaptopTemplateStatus.Done || next == LaptopTemplateStatus.Failed;
            case LaptopTemplateStatus.Running:
                switch (next)
                {
                    case LaptopTemplateStatus.Failed:
                        return true;
                }
                return false;
            case LaptopTemplateStatus.Done:
                switch (next)
                {
                    case LaptopTemplateStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public LaptopTemplate Copy()
    {
        return new LaptopTemplate(Code, Title, Grade, Department, Unit, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
