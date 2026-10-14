namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception summary is in its life cycle.</summary>
public enum ExceptionSummaryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A exception summary. Its code is unique within a tenant.</summary>
public sealed class ExceptionSummary(string code, string name, double discount, string team, string contactEmail, string notes, int capacity, ExceptionSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public string Team { get; set; } = team;
    public string ContactEmail { get; set; } = contactEmail;
    public string Notes { get; set; } = notes;
    public int Capacity { get; set; } = capacity;
    public ExceptionSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a exception summary in status idle.</summary>
    public static ExceptionSummary NewExceptionSummary(string code, string name)
    {
        return new ExceptionSummary(code, name, 0.0, "", "", "", 0, ExceptionSummaryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionSummaryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionSummaryStatus next)
    {
        switch (Status)
        {
            case ExceptionSummaryStatus.Idle:
                switch (next)
                {
                    case ExceptionSummaryStatus.Draining:
                        return true;
                }
                return false;
            case ExceptionSummaryStatus.Warming:
                return next == ExceptionSummaryStatus.Ready || next == ExceptionSummaryStatus.Draining;
            case ExceptionSummaryStatus.Ready:
                switch (next)
                {
                    case ExceptionSummaryStatus.Draining:
                    case ExceptionSummaryStatus.Stopped:
                        return true;
                }
                return false;
            case ExceptionSummaryStatus.Draining:
                switch (next)
                {
                    case ExceptionSummaryStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Discount * (double)Capacity, 2);
    }

    public ExceptionSummary Copy()
    {
        return new ExceptionSummary(Code, Name, Discount, Team, ContactEmail, Notes, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
