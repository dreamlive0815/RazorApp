
Vue.component('carousel', {
    props: {
        id: {
            type: String,
            required: true
        },
        imgs: {
            type: Object,
            required: true
        },
        interval: Number,
        seen: Boolean,
    },
    template: '\
    <div class="carousel slide" v-if="seen" :id="id" @dblclick="dblclickHandler">\
        <ol class="carousel-indicators">\
            <li v-for="(img,index) in imgs" :data-target="\'#\' + id" :data-slide-to="index" :class="{active:index==0}"></li>\
        </ol>\
        <div class="carousel-inner">\
            <div v-for="(img,index) in imgs" :class="{item:true,active:index==0}">\
                <img class="carousel-inner" :src="img.src">\
                <div class="carousel-caption">{{ img.title }}[{{ index + 1 }}/{{ imgs.length }}]</div>\
            </div>\
        </div>\
        <a class="left carousel-control" :href="\'#\' + id" role="button" data-slide="prev">\
            <span class="glyphicon glyphicon-chevron-left" aria-hidden="true"></span>\
            <span class="sr-only">Previous</span>\
        </a>\
        <a class="right carousel-control" :href="\'#\' + id" role="button" data-slide="next">\
            <span class="glyphicon glyphicon-chevron-right" aria-hidden="true"></span>\
            <span class="sr-only">Next</span>\
        </a>\
    </div>',
    data: function() {
        return {
            autoSlide: false
        }
    },
    methods: {
        dblclickHandler: function () {
            this.switchAutoSilde();
        },
        switchAutoSilde: function() {
            if(!this.autoSlide)
                this.autoSlideOn(2000);
            else
                this.autoSlideOff();
            this.autoSlide = !this.autoSlide;
        },
        autoSlideOn: function(interval) {
            var i = parseInt(this.interval);
            if(i != 0 && !isNaN(i)) interval = i;
            $('#' + this.id).carousel({
                interval: interval
            });
            console.log('autoSlideOn', interval);
        },
        autoSlideOff: function() {
            $('#' + this.id).carousel('pause');
            console.log('autoSlideOff');
        }
    },
})

var setCarouselSlideHandler = function(id) {
    $('#' + id).on('slide.bs.carousel', function() {
        app.headText = '翻页中...';
    });
    $('#' + id).on('slid.bs.carousel', function() {
        app.headText = defaultHeadText;
    });
}