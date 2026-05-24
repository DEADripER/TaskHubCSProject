namespace TaskHub.Core;

public delegate bool EntityPredicate<in T>(T entity);
public delegate void TaskNotification<in T>(T task);
