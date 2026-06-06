namespace SixOsTL.MVC.Extensions
{
    public sealed class LimitedStream(Stream inner, long limit) : Stream
    {
        private long _remaining = limit;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => limit;
        public override long Position
        {
            get => limit - _remaining;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_remaining <= 0) return 0;
            var toRead = (int)Math.Min(count, _remaining);
            var read = inner.Read(buffer, offset, toRead);
            _remaining -= read;
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            if (_remaining <= 0) return 0;
            var toRead = (int)Math.Min(count, _remaining);
            var read = await inner.ReadAsync(buffer.AsMemory(offset, toRead), ct);
            _remaining -= read;
            return read;
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing) inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
