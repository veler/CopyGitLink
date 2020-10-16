#nullable enable

using CopyGitLink.Shared;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;

namespace CopyGitLink.OutOfProc.Composition
{
    internal sealed class MefHostCompositionService : ICompositionService, IDisposable
    {
        private readonly CompositionBatch _compositionBatch;

        private bool _isDisposed;
        private bool _imported;

        internal static MefHostCompositionService Instance { get; } = new MefHostCompositionService();

        internal CompositionContainer Container { get; }

        private MefHostCompositionService()
        {
            Assembly serviceAssembly = Assembly.GetExecutingAssembly();

            AggregateCatalog compositionCatalog = new AggregateCatalog();
            compositionCatalog.Catalogs.Add(new AssemblyCatalog(serviceAssembly));
            compositionCatalog.Catalogs.Add(new AssemblyCatalog(typeof(PkgIds).Assembly));

            Container = new CompositionContainer(compositionCatalog);
            _compositionBatch = new CompositionBatch();

            // Compose MEF.
            Container.Compose(_compositionBatch);
        }

        public void SatisfyImportsOnce(ComposablePart part)
        {
            if (_imported)
            {
                return;
            }

            ThrowIfDisposed();
            Container.SatisfyImportsOnce(part);
            _imported = true;
        }

        ~MefHostCompositionService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                Container.Dispose();
            }

            _isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
