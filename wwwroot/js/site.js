// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {



    // ===== Feature 1 (Home): animated counters =====
    $(".counter").each(function () {
        var $el = $(this);
        var target = parseInt($el.data("target"), 10) || 0;
        $el.text("0");
        $({ n: 0 }).animate({ n: target }, {
            duration: 900,
            step: function (now) { $el.text(Math.floor(now)); },
            complete: function () { $el.text(target); }
        });
    });

    // ===== Feature 3 (Vessel Registration): live IMO hint, digits only =====
    $("#IMONumber").on("keyup input", function () {
        var digits = $(this).val().replace(/\D/g, "").substring(0, 7);
        $(this).val(digits);
        var complete = digits.length === 7;
        $("#imoHint")
            .text(digits.length + "/7 digits")
            .toggleClass("text-success", complete)
            .toggleClass("text-muted", !complete);
    });
    $("#IMONumber").trigger("input");

    // ===== Feature 4 (Vessel Registration): priority note for Medical / Food =====
    function updateCargoNote(animate) {
        var cargo = $("#CargoType option:selected").text().trim();
        var priority = (cargo === "Medical" || cargo === "Food");
        if (priority) { animate ? $("#cargoPriorityNote").slideDown() : $("#cargoPriorityNote").show(); }
        else          { animate ? $("#cargoPriorityNote").slideUp()   : $("#cargoPriorityNote").hide(); }
    }
    $("#CargoType").on("change", function () { updateCargoNote(true); });
    updateCargoNote(false);

    // ===== Feature 2 (Vessel Registration): live search in the vessels table =====
    $("#vesselSearch").on("keyup", function () {
        var text = $(this).val().toLowerCase();
        $("#vesselsTable .vessel-row").each(function () {
            var $row = $(this);
            var rowText = $row.find("td").not(":last").text().toLowerCase();  // ignore the Edit button
            var match = rowText.indexOf(text) > -1;
            $row.toggle(match);
            if (!match) { $row.next("tr.collapse").removeClass("show"); }     // close its edit form too
        });
    });

    // ===== Feature 7 (My Requests): live search + status filter + counter =====
    function filterRequests() {
        var text = $("#searchBox").val().toLowerCase();
        var status = $("#statusFilter").val();
        var $rows = $("#requestsTable tbody tr");
        var visible = 0;

        $rows.each(function () {
            var $row = $(this);
            var matchText = $row.text().toLowerCase().indexOf(text) > -1;
            var matchStatus = !status || $row.find(".status-cell").text().trim() === status;
            var show = matchText && matchStatus;
            $row.toggle(show);
            if (show) { visible++; }
        });
        $("#rowCount").text(visible + " of " + $rows.length);
    }
    $("#searchBox").on("keyup", filterRequests);
    $("#statusFilter").on("change", filterRequests);
    if ($("#requestsTable").length) { filterRequests(); }

    // ===== Feature 8 (Request Approval): confirm before approve / reject =====
    $(".btn-approve").on("click", function () {
        return confirm("Approve this request?");
    });
    $(".btn-reject").on("click", function () {
        return confirm("Reject this request?");
    });

    // ===== Feature 9 (Berth Management): slide the Add Berth form =====
    $("#toggleAddForm").on("click", function () {
        $("#addBerthCard").slideToggle();
    });
    // reopen the form if a failed submit left text in it
    if ($("#BerthName").length && $("#BerthName").val().trim() !== "") {
        $("#addBerthCard").show();
    }

    // ===== Feature 5 (Berth Request): vessel details loaded with AJAX =====
    $("#VesselId").on("change", function () {
        var id = $(this).val();
        var $info = $("#vesselInfo");

        if (!id || id === "0") { $info.empty(); return; }

        $.get("?handler=VesselInfo", { id: id }, function (v) {
            if (!v.found) { $info.empty(); return; }

            var badgeClass = v.priority === "High" ? "bg-danger"
                           : v.priority === "Medium" ? "bg-warning text-dark"
                           : "bg-secondary";

            $info.empty()
                .append($("<span>").text("IMO: " + v.imo + " | Cargo: " + v.cargo + " | Agent: " + v.agent + " "))
                .append($("<span>").addClass("badge " + badgeClass).text(v.priority + " priority"));
        }).fail(function () {
            $("#vesselInfo").text("Could not load vessel details.");
        });
    });

    // ===== Feature 6 (Berth Request): availability check without reloading =====
    function checkAvailability() {
        var vesselId = $("#VesselId").val();
        var berthId = $("#BerthId").val();
        var arrival = $("#ArrivalTime").val();
        var departure = $("#DepartureTime").val();
        var $result = $("#availabilityResult");

        if (!vesselId || vesselId === "0" || !berthId || berthId === "0" || !arrival || !departure) {
            $result.empty();
            return;
        }

        $.get("?handler=Check",
            { vesselId: vesselId, berthId: berthId, arrival: arrival, departure: departure },
            function (r) {
                var $box = $("<div>")
                    .addClass("alert " + (r.ok ? "alert-success" : "alert-danger"))
                    .text(r.message);

                if (r.suggestedArrival) {
                    $box.append("<br>")
                        .append(document.createTextNode("Nearest free time: " + r.suggestionText + " "))
                        .append($("<button type='button' id='useSlot' class='btn btn-sm btn-primary ms-2'>Use this time</button>")
                            .attr("data-arrival", r.suggestedArrival)
                            .attr("data-departure", r.suggestedDeparture));
                }

                $result.hide().empty().append($box).fadeIn();
            }
        ).fail(function () {
            $("#availabilityResult").text("Could not check availability. Please try again.");
        });
    }

    $("#VesselId, #BerthId, #ArrivalTime, #DepartureTime").on("change", checkAvailability);

    // "Use this time" button fills the suggested slot
    $(document).on("click", "#useSlot", function () {
        $("#ArrivalTime").val($(this).attr("data-arrival"));
        $("#DepartureTime").val($(this).attr("data-departure"));
        checkAvailability();
    });

    // ===== Feature 10 (All pages): highlight priority rows + fade out success messages =====
    $("#approvalTable .priority-cell").each(function () {
        var $row = $(this).closest("tr");
        var isHigh = $(this).text().trim() === "High";
        var isPending = $row.find(".btn-approve").length > 0;   // only pending rows have buttons
        if (isHigh && isPending) { $row.addClass("table-warning"); }
    });

    setTimeout(function () { $(".auto-hide").fadeOut(600); }, 4000);
});