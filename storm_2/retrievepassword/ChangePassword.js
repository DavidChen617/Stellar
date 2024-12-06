$('#sendmail').on('submit',function(event){
    event.preventDefault();
    $.ajax({
        type: "POST",
        url: "/Authentication/SendMail",
        data: "",
        dataType: "application/json",
        success: function (response) {
            console.log("成功寄送");
        }
    });
})

