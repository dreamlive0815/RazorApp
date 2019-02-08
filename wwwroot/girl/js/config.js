
var config = {
    apiUrlPrefix: '',
    pageUrlPrefix: './',
    defaultAutoSlideInterval: 5000,
    defaultHeadText: 'ZNGirls',

    getAlbumsPageUrl: function(girlId) {
        return this.pageUrlPrefix + 'albums.html?girlId=' + girlId;
    },
    getAlbumInfoPageUrl: function(albumId) {
        return this.pageUrlPrefix + 'albuminfo.html?albumId=' + albumId;
    }
};