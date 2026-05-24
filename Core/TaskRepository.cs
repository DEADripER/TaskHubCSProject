using TaskHub.Models;

namespace TaskHub.Core;

public class TaskRepository<T> where T : ITaskEntity
{
    private readonly List<T> _items = new();
    private readonly object _syncRoot = new();
    private int _nextId = 1;

    public void Add(T item)
    {
        lock (_syncRoot)
        {
            if (item.Id == 0)
            {
                item.Id = _nextId;
                _nextId++;
            }
            else if (item.Id >= _nextId)
            {
                _nextId = item.Id + 1;
            }

            _items.Add(item);
        }
    }

    public IReadOnlyList<T> GetAll()
    {
        lock (_syncRoot)
        {
            return _items.ToList();
        }
    }

    public T? GetById(int id)
    {
        lock (_syncRoot)
        {
            return _items.FirstOrDefault(item => item.Id == id);
        }
    }

    public IReadOnlyList<T> Find(EntityPredicate<T> predicate)
    {
        lock (_syncRoot)
        {
            return _items.Where(item => predicate(item)).ToList();
        }
    }

    public void Update(T updatedItem)
    {
        lock (_syncRoot)
        {
            var index = _items.FindIndex(item => item.Id == updatedItem.Id);
            if (index < 0)
            {
                throw new InvalidOperationException("Элемент не найден.");
            }

            _items[index] = updatedItem;
        }
    }

    public bool Remove(int id)
    {
        lock (_syncRoot)
        {
            var item = _items.FirstOrDefault(task => task.Id == id);
            return item is not null && _items.Remove(item);
        }
    }

    public void ReplaceAll(IEnumerable<T> items)
    {
        lock (_syncRoot)
        {
            _items.Clear();
            _items.AddRange(items);
            _nextId = _items.Count == 0 ? 1 : _items.Max(item => item.Id) + 1;
        }
    }
}
