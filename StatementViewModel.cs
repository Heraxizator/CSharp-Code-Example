namespace DvgupsMobile.ViewModels.JornalViewModels.ElectronStatement
{
    public abstract class StatementViewModel : BaseViewModel
    {
        private bool loading { get; set; }
        private bool loaded { get; set; }
        private bool failed { get; set; }

        protected enum Mode
        {
            Loading,
            Loaded,
            Failed
        }

        protected void Default()
        {
            this.Loading = false;
            this.Loaded = false;
            this.Failed = false;
        }

        protected void Animation(Mode mode)
        {
            Default();

            switch (mode)
            {
                case Mode.Loading:
                    this.Loading = true;
                    break;
                case Mode.Loaded:
                    this.Loaded = true;
                    break;
                case Mode.Failed:
                    this.Failed = true;
                    break;
            }
        }

        public bool Loading
        {
            get => this.loading;
            set
            {
                if (this.loading != value)
                {
                    this.loading = value;
                    OnPropertyChanged(nameof(this.Loading));
                }
            }
        }

        public bool Loaded
        {
            get => this.loaded;
            set
            {
                if (this.loaded != value)
                {
                    this.loaded = value;
                    OnPropertyChanged(nameof(this.Loaded));
                }
            }
        }

        public bool Failed
        {
            get => this.failed;
            set
            {
                if (this.failed != value)
                {
                    this.failed = value;
                    OnPropertyChanged(nameof(this.Failed));
                }
            }
        }
    }
}
