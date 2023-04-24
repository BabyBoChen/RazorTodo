# 待辦事項

使用Material Design的web元件搭配Core Razor Page架構所撰寫的一個記錄待辦事項的網頁應用程式。該網站使用Entity Framework Core資料存取技術搭配SQLite資料庫來儲存待辦事項，並已上架至微軟的Azure雲端平台上。


「開啟相簿」功能使用Dropbox Api與Dropbox結合，可以從「待辦事項」網站直接上傳照片至Dropbox雲端硬碟，並將照片嵌入於網站。


將照片上傳至雲端硬碟（Dropbox或Google Drive）並嵌入於網站中可以減少使用網站伺服器的流量（outbound traffic），避免網站因達到流量上限而被暫停。

https://bbljtodo.azurewebsites.net/
