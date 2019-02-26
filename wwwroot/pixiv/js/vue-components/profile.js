/*
[外部依赖]

[事件]
ontryfollow
ontryunfollow
*/

Vue.component('profile', {
    props: {
        user: {
            type: Object,
            default: {},
        },
        enablefollowstate: {
            type: Boolean,
            default: true,
        },
        follow: {
            type: Boolean,
            default: false,
        },
        visible: {
            type: Boolean,
            default: true,
        },
    },
    template: '\
<div class="container" v-if="visible">\
    <div class="thumbnail">\
        <img class="img-responsive" :src="user.avatar"></img>\
        <p class="centerp"><a target="_blank">{{ user.name }}</a></p>\
        <button class="btn btn-block" v-if="enablefollowstate" :class="{\'btn-primary\':user.follow}" @click="switchFollowState">{{ user.follow ? \'关注中\' : \'未关注\' }}</button>\
    </div>\
</div>',
    methods: {
        switchFollowState() {
            var user = this.user;
            if (user.follow)
                this.$emit('ontryunfollow', user.id);
            else
                this.$emit('ontryfollow', user.id);
        },
    },
    mounted: function() {
        this.$nextTick(function() {

        });
    },
})