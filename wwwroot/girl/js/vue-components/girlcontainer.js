/*
[外部依赖]

config: Object
util: Js
[事件]

*/

Vue.component('girlcontainer', {
    props: {
        girl: {
            type: Object,
            required: true
        },
        list: {
            type: Array,
            required: true
        },
        visible: Boolean,
        disableprofile: Boolean,
    },
    template: '\
<div class="container" :style="{display:visible ? \'block\' : \'none\' }">\
    <div v-if="!disableprofile" class="thumbnail">\
        <img class="img-responsive" :src="girl.avatar"></img>\
        <p class="centerp"><a :href="albumsPageUrl" target="_blank">{{ girl.name }}{{ girl.score ? \'[\' + girl.score + \']\' : \'\' }}</a></p>\
        <button class="btn btn-primary btn-block" @click="switchMore">更多</button>\
        <div :style="{display:moreVisible ? \'block\' : \'none\' }">\
            <table class="table table-striped" style="margin:0;">\
                <tbody>\
                    <tr v-for="(value, key) in girl">\
                        <td class="text-center">{{ firstCharUpperCase(key) }}:{{ value }}</td>\
                    </tr>\
                </tbody>\
            </table>\
        </div>\
    </div>\
    <div class="row">\
        <div v-for="item in list" class="col-xs-12 col-sm-6 col-md-4">\
            <div class="thumbnail">\
                <img class="img-responsive" style="width:100%;" :src="item.src" alt="">\
                <a class="btn btn-primary btn-block" :href="item.href" target="_blank">{{ item.title }}</a>\
            </div>\
        </div>\
    </div>\
</div>',
    data: function() {
        return {
            moreVisible: false
        }
    },
    computed: {
        albumsPageUrl: function() {
            //如果已经是当前页面则设置链接无效
            if(window.location.href.indexOf(config.getAlbumsPageUrl(this.girl.id).substr(1)) != -1)
                return 'javascript:void(0)';
            return config.getAlbumsPageUrl(this.girl.id);
        }
    },
    methods: {
        firstCharUpperCase: function(s) {
            return firstCharUpperCase(s);
        },
        switchMore: function() {
            this.moreVisible = !this.moreVisible;
        }
    },
    mounted: function() {
        this.$nextTick(function() {

        });
    },
})
