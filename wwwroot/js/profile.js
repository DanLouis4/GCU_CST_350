$(document).on("click", "#editProfile", function () {
    $("#profile-container").load("/User/EditProfile");
});

$(document).on("submit", "#profileEditForm", function (e) {
    e.preventDefault();

    $.ajax({
        url: "/User/UpdateProfile",
        type: "POST",
        data: $(this).serialize(),
        success: function (html) {
            $("#profile-container").html(html);
        }
    });
});
