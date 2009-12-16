using CodeCampServer.Core.Bases;

namespace CodeCampServer.Core.Domain.Model
{
	public abstract class AuditedPersistentObject : PersistentObject, IAuditable
	{
		private ChangeAuditInfo _changeAuditInfo;

		protected AuditedPersistentObject()
		{
			_changeAuditInfo = new ChangeAuditInfo();
		}

		public virtual ChangeAuditInfo ChangeAuditInfo
		{
			get
			{
				if (_changeAuditInfo == null)
					_changeAuditInfo = new ChangeAuditInfo();

				return _changeAuditInfo;
			}
			set
			{
				if (value == null)
					return;

				_changeAuditInfo = value;
			}
		}
	}
}