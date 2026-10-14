namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment exception is in its life cycle.</summary>
public enum GarnishmentExceptionStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A garnishment exception as the garnishment team keeps it.</summary>
public sealed class GarnishmentException(string code, string title, string name, double maximum, string grade, double cost, int version, int startsAt, double fte, GarnishmentExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string Grade { get; set; } = grade;
    public double Cost { get; set; } = cost;
    public int Version { get; set; } = version;
    public int StartsAt { get; set; } = startsAt;
    public double Fte { get; set; } = fte;
    public GarnishmentExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment exception in status new.</summary>
    public static GarnishmentException NewGarnishmentException(string code, string title)
    {
        return new GarnishmentException(code, title, "", 0.0, "", 0.0, 0, 0, 0.0, GarnishmentExceptionStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentExceptionStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentExceptionStatus next)
    {
        switch (Status)
        {
            case GarnishmentExceptionStatus.New:
                switch (next)
                {
                    case GarnishmentExceptionStatus.Done:
                    case GarnishmentExceptionStatus.Failed:
                        return true;
                }
                return false;
            case GarnishmentExceptionStatus.Queued:
                switch (next)
                {
                    case GarnishmentExceptionStatus.Done:
                    case GarnishmentExceptionStatus.Failed:
                        return true;
                }
                return false;
            case GarnishmentExceptionStatus.Running:
                return next == GarnishmentExceptionStatus.Done || next == GarnishmentExceptionStatus.Failed;
            case GarnishmentExceptionStatus.Done:
                switch (next)
                {
                    case GarnishmentExceptionStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"garnishment exception"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Cost * (double)Version, 2);
    }

    public GarnishmentException Copy()
    {
        return new GarnishmentException(Code, Title, Name, Maximum, Grade, Cost, Version, StartsAt, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
