using System;
using System.Net.Http.Headers;
using Microsoft.VisualBasic;

class Program
{
    static void Main()
    {
        try
        {
            // I have used the homeworks example, but since it was already ordered by the weights,
            // I have decided to change the order of data a little bit
            int[] deadlines = {4, 4, 5, 4, 3, 5, 1, 8, 7, 1};
            int[] weights = {6, 4, 8, 1, 10, 4, 8, 2, 3, 7};
            var schedule = new Schedule(deadlines, weights);

            var matroid = new Matroid(schedule);
            var optimalSet = matroid.GetOptimalSet();
            optimalSet.PrintTaskDataAsSolution();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

public class Matroid
{
    public TaskData[] tasks;

    public Matroid(Schedule schedule)
    {
        tasks = new TaskData[schedule.tasks.Length];
        for(int i = 0; i < schedule.tasks.Length; i++)
        {
            tasks[i] = new TaskData(schedule.tasks[i]);
        }
    }

    public TaskData[] GetOptimalSet()
    {
        var optimalSet = new TaskData[tasks.Length];
        var lateTasks = new List<TaskData>();
        var length = optimalSet.Length;

        // this orders tasks based on weights in descending order
        // this uses Quicksort algorithm under the hood
        tasks = tasks.OrderByDescending(x => x.weight).ToArray();
        tasks.PrintTaskData();

        // filling optimal set with 0's
        for (int i = 0; i < length; i++)
            optimalSet[i] = TaskData.empty();

        for (int i = 0; i < length; i++)
        {
            int insertPos = tasks[i].deadline - 1;
            while (insertPos >= 0 && optimalSet[insertPos].index != 0)
                insertPos--;

            if (insertPos >= 0)
                optimalSet[insertPos] = tasks[i];
            else
                lateTasks.Add(tasks[i]);
        }
        optimalSet.PrintTaskData();
        ToEarlyFirstForm(optimalSet, lateTasks.Count);
        ToCanonicalForm(optimalSet, length - lateTasks.Count);

        // after this 2 steps, we have managed to fill the optimal set, with only no late tasks
        // the tasks that are going to be late are not still added, istead there is empty cells for them
        // we can just go over them, and will them with late tasks
        for(int i = 0; i < lateTasks.Count; i++)
            optimalSet[length - 1 - i] = lateTasks[i];
        return optimalSet;
    }

    private void ToEarlyFirstForm(TaskData[] set, int numofLate)
    {
        int lastIndex = set.Length - 1;

        // this is for moving empty cells, which are the late tasks to the end of the set
        for(int i = 0; i < numofLate; i++)
        {
            if (set[lastIndex - i].isEmpty)
                continue ;

            var j = i + 1;
            while (j <= lastIndex && !set[lastIndex - j].isEmpty)
                j++;

            if (j <= lastIndex)
                TaskData.Swap(ref set[lastIndex - i], ref set[lastIndex - j]);
            else
                throw new InvalidOperationException("Something unexpected, maybe I deserve to get 0?!!");
        }
    }

    private void ToCanonicalForm(TaskData[] set, int size)
    {
        // here we are taking only the independent sets: the tasks that can be managed
        //and order them by the deadlines 
        set.Take(size).OrderBy(x => x.deadline);
    }
}

public class TaskData
{
    public int index;
    public int deadline;
    public int weight;

    public TaskData(TaskData src)
    {
        index = src.index;
        deadline = src.deadline;
        weight = src.weight;
    }

    public TaskData(int _index, int _deadline, int _weight)
    {
        index = _index;
        deadline = _deadline;
        weight = _weight;
    }

    public static TaskData empty()
    {
        return new TaskData(0, 0, 0);
    }

    public bool isEmpty {get => index == 0;}
    public static void Swap(ref TaskData first, ref TaskData second)
    {
        var temp = first;
        first = second;
        second = temp;
    }
    
    public void PrintData(string ending)
    {
        Console.Write($"   | i: {index}, d: {deadline}, w: {weight} | {ending}");
    }
}

public class Schedule
{
    public TaskData[] tasks;
    private int currentIndex = 0;

    public Schedule(int[] deadlines, int[] weights)
    {
        if (deadlines.Length != weights.Length)
            throw new InvalidDataException("Cound not create schedule for the given input!!\nDeadlines and weights are of different sizes!!\n");

        tasks = new TaskData[deadlines.Length];
        for (int i = 0; i < deadlines.Length; i++)
            tasks[i] = new TaskData(i + 1, deadlines[i], weights[i]);
    }

    public Schedule(int size)
    {
        tasks = new TaskData[size];
    }

    public Schedule()
    {
        // some default data
        tasks = new TaskData[10];
    }

    public void InsertElement(TaskData newData)
    {
        if (currentIndex >= tasks.Length)
            throw new InvalidDataException("Cannot Insert the data, Schedule is full!!\n");

        tasks[currentIndex++] = newData;
    }
}

public static class TasksExtention
{
    public static void PrintTaskData(this TaskData[] tasks)
    {
        Console.WriteLine("   |-----------------------|");
        foreach(var task in tasks)
            task.PrintData("\n");
        Console.WriteLine("   |-----------------------|");
    }

    public static void PrintTaskDataAsSolution(this TaskData[] optimalSet)
    {
        int totalPenalty = 0;
        for(int i = 0; i < optimalSet.Length; i++)
        {
            optimalSet[i].PrintData("");
            if (optimalSet[i].deadline >= i + 1)
                Console.WriteLine("Manages |");
            else
            {
                totalPenalty += optimalSet[i].weight;
                Console.WriteLine("Late |");
            }
        }
        Console.WriteLine("   |-----------------------|");
        Console.WriteLine($"    Total Penalty: {totalPenalty}");
        Console.WriteLine("   |-----------------------|");

    }
}