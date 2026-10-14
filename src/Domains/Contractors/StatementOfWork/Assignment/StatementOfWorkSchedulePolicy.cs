namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of statement of work schedule policy.</summary>
public static class StatementOfWorkSchedulePolicy
{
    public static int StatementOfWorkScheduleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string StatementOfWorkScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 31396.0, in order.</summary>
    public static int StatementOfWorkScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31396.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
