namespace TooNet;

/// <summary>
/// A buffer writer that uses ArrayPool for efficient memory management.
/// </summary>
internal sealed class PooledBufferWriter : IBufferWriter<byte>, IDisposable
{
    private byte[] _buffer;
    private int _written;
    private bool _disposed;

    public PooledBufferWriter(int initialCapacity = 4096)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Initial capacity must be positive");
        }

        _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
        _written = 0;
    }

    public int WrittenCount => _written;

    public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, _written);

    public ReadOnlyMemory<byte> WrittenMemory => _buffer.AsMemory(0, _written);

    public void Advance(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative");
        }

        if (_written + count > _buffer.Length)
        {
            throw new InvalidOperationException("Cannot advance past buffer size");
        }

        _written += count;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsMemory(_written);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsSpan(_written);
    }

    private void EnsureCapacity(int sizeHint)
    {
        if (sizeHint < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeHint), "Size hint cannot be negative");
        }

        if (_written + sizeHint <= _buffer.Length)
        {
            return;
        }

        var newSize = Math.Max(_buffer.Length * 2, _written + sizeHint);

        // Protect against overflow
        if (newSize < 0)
        {
            newSize = int.MaxValue;
        }

        var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
        _buffer.AsSpan(0, _written).CopyTo(newBuffer);

        ArrayPool<byte>.Shared.Return(_buffer, clearArray: false);
        _buffer = newBuffer;
    }

    public byte[] ToArray()
    {
        return _buffer.AsSpan(0, _written).ToArray();
    }

    public void Clear()
    {
        _written = 0;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            ArrayPool<byte>.Shared.Return(_buffer, clearArray: true);
            _buffer = Array.Empty<byte>();
            _written = 0;
            _disposed = true;
        }
    }
}