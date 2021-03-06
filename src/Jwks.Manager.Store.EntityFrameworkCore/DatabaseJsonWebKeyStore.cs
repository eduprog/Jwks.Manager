using Jwks.Manager.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jwks.Manager.Store.EntityFrameworkCore
{
    internal class DatabaseJsonWebKeyStore<TContext> : IJsonWebKeyStore
        where TContext : DbContext, ISecurityKeyContext
    {
        private readonly TContext _context;
        private readonly IOptions<JwksOptions> _options;
        private readonly ILogger<DatabaseJsonWebKeyStore<TContext>> _logger;

        public DatabaseJsonWebKeyStore(TContext context, ILogger<DatabaseJsonWebKeyStore<TContext>> logger, IOptions<JwksOptions> options)
        {
            _context = context;
            _options = options;
            _logger = logger;
        }

        public void Save(SecurityKeyWithPrivate securityParamteres)
        {
            _context.SecurityKeys.Add(securityParamteres);

            _logger.LogInformation($"Saving new SecurityKeyWithPrivate {securityParamteres.Id}", typeof(TContext).Name);
            _context.SaveChanges();
        }

        public SecurityKeyWithPrivate GetCurrentKey()
        {
            // Put logger in a local such that `this` isn't captured.
            return _context.SecurityKeys.OrderByDescending(d => d.CreationDate).AsNoTracking().FirstOrDefault();
        }

        public IReadOnlyCollection<SecurityKeyWithPrivate> Get(int quantity = 5)
        {
            return _context.SecurityKeys.OrderByDescending(d => d.CreationDate).Take(quantity).AsNoTracking().ToList().AsReadOnly();
        }

        public void Clear()
        {

        }

        public bool NeedsUpdate()
        {
            var current = GetCurrentKey();
            if (current == null)
                return true;

            return current.CreationDate.AddDays(_options.Value.DaysUntilExpire) < DateTime.UtcNow.Date;
        }

        public void Update(SecurityKeyWithPrivate securityKeyWithPrivate)
        {
            _context.SecurityKeys.Update(securityKeyWithPrivate);
            _context.SaveChanges();
        }

    }
}