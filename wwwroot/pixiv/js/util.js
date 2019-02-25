
function firstCharUpperCase(s)
{
    if(typeof(s) != 'string') s = s.toString();
    if(s.length == 0) return s;
    return s[0].toUpperCase() + s.substr(1);
}

//获取url参数
function getQueryParams()
{
    var res = {};
    var query = window.location.search;
    if(query.indexOf('?') != -1) {
        query = query.substr(1);
        var pairs = query.split("&");
        for(var i = 0; i < pairs.length; i++) {
            var pair = pairs[i];
            var kv = pair.split('=');
            var key = kv[0]; var value = kv.length > 1 ? kv[1] : "";
            res[key] = unescape(value);
        }
    }
    return res;
}

function has(obj, property)
{
    if(typeof(obj) != 'object') return false;
    return obj.hasOwnProperty(property);
}

//刷新当前页面
function refreshPage()
{
    window.location.reload();
}
