/*
[外部依赖]
bootstrap
[事件]
onconfirm:点击确认之后触发
*/

Vue.component('modal', {
    props: {
        body: String,
        id: {
            type: String,
            required: true
        },
        title: String,
    },
    template: '\
        <div class="modal fade" tabindex="-1" role="dialog" :id="id">\
            <div class="modal-dialog" role="document">\
                <div class="modal-content">\
                    <div class="modal-header">\
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
                        <h4 class="modal-title">{{ title }}</h4>\
                    </div>\
                    <div class="modal-body">{{ body }}</div>\
                    <div class="modal-footer">\
                        <button type="button" class="btn btn-default" data-dismiss="modal">取消</button>\
                        <button type="button" class="btn btn-primary" data-dismiss="modal" @click="confirmHandler">确定</button>\
                    </div>\
                </div>\
            </div>\
        </div>',
    methods: {
        confirmHandler: function () {
            this.$emit('onconfirm');
        },
        show: function(title, body) {
            this.title = title;
            this.body = body;
            $('#' + this.id).modal();
        },
    },
})