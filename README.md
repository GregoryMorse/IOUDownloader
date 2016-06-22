# IOUDownloader
Islamic Online University Course Downloader Written in .NET

https://github.com/GregoryMorse/IOUDownloader/blob/master/bin/Release/IOUDownloaderMerged.exe?raw=true

On Windows, if you do not have .NET 4.0 choose one of the following:
 .NET 4.0 Full Installer: http://www.microsoft.com/en-in/download/details.aspx?id=17718
 .NET 4.0 Web Installer: http://www.microsoft.com/en-us/download/details.aspx?id=17851

The IOU Downloader supports Mac and Linux. To run it, simply download the same file above.  Also download and install Mono.NET with Visual Basic library for Mac or Linux (preferably the Xamarin one unless you are a system configuration expert): http://www.mono-project.com/download/
 It will need to be properly installed with the Visual Basic package and certificate packages installed for the https (SSL) parts of the IOU site to download.
 Eventually you just run: "mono IOUDownloaderMerged.exe" and it will work properly if you have configured Mono.NET

Example for Linux:
Install Mono with Visual Basic package depending on which Linux you are using here: http://www.mono-project.com/docs/getting-started/install/linux/
For example after installing the latest Ubuntu 14.4.4 on a virtual machine, installing all updates, I ran in a terminal:
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list
sudo apt-get update
sudo apt-get install mono-complete
sudo apt-get install libmono-microsoft-visualbasic10.0-cil

Example for Mac:
Download install Mono:
http://www.mono-project.com/download/#download-mac
Download IOUDownloaderMerged.exe from above.
Then open a Terminal command prompt then "cd Downloads" to go to where it is downloaded for example.  Then execute "mono IOUDownloaderMerged.exe" and be patient.
