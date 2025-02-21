using SFML.Graphics;
using Glacc.UI.Elements;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Glacc.UI.Components
{
    public class FileSelector : Viewport
    {
        public int maxItemPerPage = 128;
        public string lastSelectedFilePath = string.Empty;

        DirectoryInfo? dirInfo;

        Button btnCancel;

        InputBox ipbPath;

        Viewport directoryListing;
        ScrollViewport directoryListingViewport;
        int widthOfDirectoryListing;

        Label pageLabel;
        Button prevBtn;
        Button nextBtn;

        bool toRelist = false;

        int currPage;
        int maxPage;

        public EventHandler<EventArgs>? onFileSelect = null;
        public EventHandler<EventArgs>? onCancel = null;

        public void SetBgColor(uint color)
        {
            Color newColor = new Color(color);
            bgColor = newColor;
            directoryListing.bgColor = newColor;
            directoryListingViewport.bgColor = newColor;

            /*
            uint colorTransparent = color & 0x00000000;
            directoryListing.bgColor = new Color(colorTransparent);
            directoryListingViewport.bgColor = new Color(colorTransparent);
            */
        }

        string GetFileDirectoryInternal(string path)
            => string.Join('\\', path.Split("\\").SkipLast(1));

        void OnPathEntered(object? sender, EventArgs e)
        {
            if (sender == null)
                return;

            InputBox? inputBox = sender as InputBox;
            if (inputBox == null)
                return;

            string path = inputBox.text;
            if (SetDir(path))
                ListDir(0, maxItemPerPage);
        }

        public bool SetDir(string? path)
        {
            string assemblyPath = GetFileDirectoryInternal(Assembly.GetExecutingAssembly().Location);

            if (path != null)
            {
                try
                {
                    bool filePathExist = false;
                    FileAttributes attributes = File.GetAttributes(path);

                    if (attributes.HasFlag(FileAttributes.Directory))
                    {
                        if (Directory.Exists(path))
                            filePathExist = true;
                    }
                    else
                    {
                        if (File.Exists(path))
                        {
                            filePathExist = true;
                            path = GetFileDirectoryInternal(path);
                        }
                    }

                    /*
                    switch (attributes)
                    {
                        case FileAttributes.Directory:
                            if (Directory.Exists(path))
                                filePathExist = true;

                            break;
                        default:
                            if (File.Exists(path))
                            {
                                filePathExist = true;
                                path = GetFileDirectoryInternal(path);
                            }
                            break;
                    }
                    */

                    if (filePathExist)
                    {
                        dirInfo = new DirectoryInfo(path);
                        return true;
                    }
                }
                catch { }

                return false;
            }

            dirInfo = new DirectoryInfo(assemblyPath);

            return true;
        }

        void OnItemSelect(object? sender, EventArgs e)
        {
            if (sender == null)
                return;

            Button? button = sender as Button;
            if (button == null)
                return;

            string[] customData = button.customData.Split('\n');
            if (customData[0] == "dir")
            {
                string dirString = customData[1];

                dirInfo = new DirectoryInfo(dirString);

                toRelist = true;
            }
            if (customData[0] == "drives")
            {
                dirInfo = null;

                toRelist = true;
            }
            if (customData[0] == "file")
            {
                string fileString = customData[1];

                // Console.WriteLine($"File Clicked: {fileString}");

                lastSelectedFilePath = fileString;
                if (onFileSelect != null)
                    onFileSelect.Invoke(this, EventArgs.Empty);
            }
        }

        void OnCancelClicked(object? sender, EventArgs e)
        {
            if (onCancel != null)
                onCancel.Invoke(this, EventArgs.Empty);
        }

        void OnPrevClicked(object? sender, EventArgs e)
        {
            if (currPage > 0)
                currPage--;

            ListDir(currPage, maxItemPerPage);
        }

        void OnNextClicked(object? sender, EventArgs e)
        {
            if (currPage < maxPage)
                currPage++;

            ListDir(currPage, maxItemPerPage);
        }

        void ListDir(int page = 0, int maxItemPerPage = 128)
        {
            directoryListing.elements.Clear();

            int btnSpacing = 4;
            int btnHeight = 24;
            int btnWidth = directoryListing.width - (btnSpacing * 2);
            int btnYInc = btnHeight + btnSpacing;

            int btnX = btnSpacing;
            int btnY = btnSpacing;

            if (dirInfo == null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    DriveInfo[] listOfDriveInfo = DriveInfo.GetDrives();

                    ipbPath.text = string.Empty;

                    directoryListing.width = directoryListing.width;
                    directoryListing.height = btnSpacing + (btnYInc * listOfDriveInfo.Length);
                    directoryListing.UpdateSize();

                    currPage = 0;
                    maxPage = 0;

                    foreach (DriveInfo driveInfo in listOfDriveInfo)
                    {
                        Button driveBtn = new Button($"[drive] {driveInfo.Name}", btnX, btnY, btnWidth, btnHeight);
                        driveBtn.customData = "dir\n" + driveInfo.Name;
                        driveBtn.textAlign = TextAlign.Left;
                        driveBtn.onClick += OnItemSelect;

                        directoryListing.elements.Add(driveBtn);

                        btnY += btnYInc;
                    }
                }

                goto ListDir_End;
            }

            DirectoryInfo? parentDirInfo = dirInfo.Parent;

            ipbPath.text = dirInfo.FullName;

            int preservedItems = 0;
            /* Go to parent */
            Button btnGoParent = new Button("[parent]", btnX, btnY, btnWidth, btnHeight);
            btnGoParent.textAlign = TextAlign.Left;
            if (parentDirInfo != null)
            {
                string parentDirectory = parentDirInfo.FullName;
                btnGoParent.customData = "dir\n" + parentDirectory;
            }
            else
                btnGoParent.customData = "drives\n";
            btnGoParent.onClick += OnItemSelect;
            directoryListing.elements.Add(btnGoParent);
            preservedItems++;
            btnY += btnYInc;

            /* Go to root */
            Button? btnGoRoot = null;
            if (dirInfo.FullName != dirInfo.Root.FullName)
            {
                btnGoRoot = new Button("[root]", btnX, btnY, btnWidth, btnHeight);
                btnGoRoot.textAlign = TextAlign.Left;
                btnGoRoot.customData = "dir\n" + dirInfo.Root;
                btnGoRoot.onClick += OnItemSelect;
                directoryListing.elements.Add(btnGoRoot);
                preservedItems++;
                btnY += btnYInc;
            }

            try
            {
                /* Listing directory items */
                DirectoryInfo[] listOfDirInfo = dirInfo.GetDirectories();
                FileInfo[] listOfFileInfo = dirInfo.GetFiles();

                int numOfItems = listOfDirInfo.Length + listOfFileInfo.Length;
                int numOfitemsToList = ((numOfItems > maxItemPerPage) ? maxItemPerPage : numOfItems);

                if (numOfItems > 0)
                {
                    maxPage = numOfItems / maxItemPerPage;
                    if (numOfItems % maxItemPerPage != 0)
                        maxPage++;
                    maxPage--;
                }
                else
                    maxPage = 0;

                if (page < 0)
                    page = 0;
                if (page > maxPage)
                    page = maxPage;

                if (page == maxPage)
                    numOfitemsToList = numOfItems % maxItemPerPage;
                if (numOfitemsToList == 0)
                    numOfitemsToList = maxItemPerPage;

                int newDirectoryListingHeight = btnSpacing + (btnYInc * (numOfitemsToList + preservedItems));
                directoryListing.width = directoryListing.width;
                directoryListing.height = newDirectoryListingHeight;
                directoryListing.UpdateSize();

                int itemCount = 0;
                /* Directories */
                int itemIndex = page * maxItemPerPage;
                if (itemIndex < listOfDirInfo.Length)
                {
                    // foreach (DirectoryInfo dirInfo in listOfDirInfo)
                    while (itemIndex < listOfDirInfo.Length)
                    {
                        if (itemCount == maxItemPerPage)
                            break;

                        DirectoryInfo dirInfo = listOfDirInfo[itemIndex];

                        Button btnDirectory = new Button($"[dir] {dirInfo.Name}", btnX, btnY, btnWidth, btnHeight);
                        btnDirectory.customData = "dir\n" + dirInfo.FullName;
                        btnDirectory.textAlign = TextAlign.Left;
                        btnDirectory.onClick += OnItemSelect;

                        directoryListing.elements.Add(btnDirectory);

                        btnY += btnYInc;

                        itemIndex++;
                        itemCount++;
                    }
                }

                /* Files */
                itemIndex -= listOfDirInfo.Length;
                if (itemIndex < listOfFileInfo.Length)
                {
                    // foreach (FileInfo fileInfo in listOfFileInfo)
                    while (itemIndex < listOfFileInfo.Length)
                    {
                        if (itemCount == maxItemPerPage)
                            break;

                        FileInfo fileInfo = listOfFileInfo[itemIndex];

                        Button btnFile = new Button(fileInfo.Name, btnX, btnY, btnWidth, btnHeight);
                        btnFile.customData = "file\n" + fileInfo.FullName;
                        btnFile.textAlign = TextAlign.Left;
                        btnFile.onClick += OnItemSelect;

                        directoryListing.elements.Add(btnFile);

                        btnY += btnYInc;

                        itemIndex++;
                        itemCount++;
                    }
                }
            }
            catch (Exception exception)
            {
                directoryListing.elements.Clear();

                directoryListing.width = directoryListing.width;
                directoryListing.height = directoryListingViewport.height;
                directoryListing.UpdateSize();

                btnY = btnSpacing + btnYInc;
                if (btnGoRoot != null)
                {
                    directoryListing.elements.Add(btnGoRoot);
                    btnY += btnYInc;
                }
                directoryListing.elements.Add(btnGoParent);

                Label lblException = new Label($"{exception}", btnSpacing, btnY, 12);
                lblException.textAlign = TextAlign.TopLeft;
                directoryListing.elements.Add(lblException);
            }

        ListDir_End:

            directoryListingViewport.scrollY = 0;
            directoryListingViewport.UpdateVisableArea();

            prevBtn.enabled = (page > 0);
            nextBtn.enabled = (page < maxPage);

            pageLabel.text = $"{page + 1} / {maxPage + 1}";

            currPage = page;
        }

        void RepositionElements(bool init = false)
        {
            const int elemSpacing = 8;
            const int elemHeight = 24;

            btnCancel.px = elemSpacing;
            btnCancel.py = elemSpacing;
            btnCancel.width = 96;
            btnCancel.height = elemHeight;

            int startXOfDirectoryIpb = elemSpacing + btnCancel.width + elemSpacing;
            ipbPath.px = startXOfDirectoryIpb;
            ipbPath.py = elemSpacing;
            ipbPath.width = width - startXOfDirectoryIpb - elemSpacing;
            ipbPath.height = elemHeight;

            int widthOfDirectoryListingViewport = width - (elemSpacing * 2);
            widthOfDirectoryListing = widthOfDirectoryListingViewport - 16;
            int startYOfDirectoryListing = elemSpacing + elemHeight + elemSpacing;
            int heightOfDirectoryListing = height - startYOfDirectoryListing - elemSpacing - elemHeight - elemSpacing;
            int endYOfDirectoryListing = startYOfDirectoryListing + heightOfDirectoryListing;
            directoryListing.px = elemSpacing;
            directoryListing.py = startYOfDirectoryListing;
            directoryListing.width = widthOfDirectoryListing;
            if (init)
                directoryListing.height = 24;
            directoryListing.UpdateSize();
            directoryListingViewport.px = elemSpacing;
            directoryListingViewport.py = startYOfDirectoryListing;
            directoryListingViewport.width = widthOfDirectoryListingViewport;
            directoryListingViewport.height = heightOfDirectoryListing;
            directoryListingViewport.UpdateSize();

            if (!init)
            {
                const int btnSpacing = 4;
                int directoryListingButtonWidth = directoryListing.width - (btnSpacing * 2);

                foreach (Element? elem in directoryListing.elements)
                {
                    Button? directoryListingItem = elem as Button;
                    if (directoryListingItem != null)
                        directoryListingItem.width = directoryListingButtonWidth;
                }
            }

            const int btnWidth = 64;
            const int btnXInc = btnWidth + elemSpacing;
            int btnX = elemSpacing;
            int btnY = endYOfDirectoryListing + elemSpacing;
            prevBtn.px = btnX;
            prevBtn.py = btnY;
            btnX += btnXInc;
            pageLabel.px = btnX + (btnWidth / 2);
            pageLabel.py = btnY + (elemHeight / 2);
            btnX += btnXInc;
            nextBtn.px = btnX;
            nextBtn.py = btnY;
        }

        protected override void UpdateSizeCustom(bool diff)
            => RepositionElements();

        protected override void UpdateCustom()
        {
            if (toRelist)
            {
                ListDir();
                toRelist = false;
            }
        }

        public FileSelector(string? path, int px, int py, int width, int height) : base(px, py, width, height)
        {
            SetDir(path);

            const int elemHeight = 24;

            btnCancel = new Button("Cancel", 96, elemHeight);
            btnCancel.onClick += OnCancelClicked;
            elements.Add(btnCancel);

            ipbPath = new InputBox();
            ipbPath.onEnterPressed += OnPathEntered;
            elements.Add(ipbPath);

            directoryListing = new Viewport(128, 128);
            directoryListingViewport = new ScrollViewport(directoryListing, 128, 128);
            elements.Add(directoryListingViewport);

            const int btnWidth = 64;
            prevBtn = new Button("Prev", btnWidth, elemHeight);
            prevBtn.onClick += OnPrevClicked;
            pageLabel = new Label("", 16);
            pageLabel.textAlign = TextAlign.Center;
            nextBtn = new Button("Next", btnWidth, elemHeight);
            nextBtn.onClick += OnNextClicked;
            elements.Add(prevBtn);
            elements.Add(pageLabel);
            elements.Add(nextBtn);

            RepositionElements(true);

            ListDir();
        }
    }
}
