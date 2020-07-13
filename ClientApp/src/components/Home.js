import React, { Component, useRef } from 'react';
import axios from 'axios';
import Camera from 'react-webcam'
import Camera2 from 'react-html5-camera-photo'

const Image2 = new FormData();

export class Home extends Component {
    constructor(props) {
        super(props);
        this.state = {
            PathFolder: "",
            PathImage: "",
            GroupName: "",
            Image: null,
            DetectObjects: [],
            Image2: null,
            isCameraOpen: false,
            ImageUrl: ""
        }
        this.handleChange = this.handleChange.bind(this)
        this.handleSubmitTraining = this.handleSubmitTraining.bind(this)
        this.handleSubmitIdentify2 = this.handleSubmitIdentify2.bind(this)
        this.handleOpenImageUrl = this.handleOpenImageUrl.bind(this)
        this.handleOpenCamera = this.handleOpenCamera.bind(this)
        this.handleCloseCamera = this.handleCloseCamera.bind(this)
        this.handleTakePhoto = this.handleTakePhoto.bind(this)
    }
    dataURLtoFile(dataurl, filename) {
        var arr = dataurl.split(','),
            mime = arr[0].match(/:(.*?);/)[1],
            bstr = atob(arr[1]),
            n = bstr.length,
            u8arr = new Uint8Array(n);

        while (n--) {
            u8arr[n] = bstr.charCodeAt(n);
        }
        return new File([u8arr], filename, { type: mime });
    }
    getBase64Image(imgUrl, callback) {
        var img = new Image();
        // onload fires when the image is fully loadded, and has width and height
        img.onload = function () {
            var canvas = document.createElement("canvas");
            canvas.width = img.width;
            canvas.height = img.height;
            var ctx = canvas.getContext("2d");
            ctx.drawImage(img, 0, 0);
            var dataURL = canvas.toDataURL("image/png"),
                dataURL = dataURL.replace(/^data:image\/(png|jpg);base64,/, "");
            callback(dataURL); // the base64 string
        };
        // set attributes and src 
        img.setAttribute('crossOrigin', 'anonymous'); //
        img.src = imgUrl;
    }

    handleOpenCamera = () => {
        this.setState({ isCameraOpen: true });
    }
    handleCloseCamera = () => {
        this.setState({ isCameraOpen: false });
    }
    handleTakePhoto = () => {
        var x = this.dataURLtoFile(this.refs.webcam.getScreenshot(), 'x.jpg')// convert base 64 to file object
        Image2.delete("Image2"); //open new image => delete the previous image
        Image2.append("Image2", x); //append the new one to send
        this.setState({
            Image: this.refs.webcam.getScreenshot(),
            DetectObjects: [], //open new image => delete the previous detections.
            Image2: Image2,
            isCameraOpen: false
        });
    }
    handleChange = event => {
        this.setState({ [event.target.name]: event.target.value });
    }
    handleOpenImage = event => {
        Image2.delete("Image2"); //open new image => delete the previous image
        Image2.append("Image2", event.target.files[0]); //append the new one to send
        this.setState({
            Image: URL.createObjectURL(event.target.files[0]),
            DetectObjects: [], //open new image => delete the previous detections.
            Image2: Image2,
            ImageUrl: ""
        });
    }
    handleOpenImageUrl = event => {
        event.preventDefault();
        Image2.delete("Image2"); //open new image => delete the previous image
        this.setState({
            Image: this.state.ImageUrl,
            Image2: null,
            DetectObjects: [] //open new image => delete the previous detections.
        });
    }

    handleSubmitTraining = event => {
        event.preventDefault();
        axios.post("/api/Face/addfaces", this.state)
            .then(console.log("path is", this.state))
            .then(res => console.log(res))
            .catch(error => console.log(error))
    }
    handleSubmitIdentify2 = event => {
        event.preventDefault();
        if (this.state.ImageUrl == "") {
            axios.post("/api/Face/identify2", Image2)
                .then(res => { console.log(res); this.setState({ DetectObjects: res.data }) })
                .catch(error => console.log(error))
        }
        else {
            axios.post("/api/Face/identifyurl", this.state)
                .then(console.log("path is", this.state))
                .then(res => { console.log(res); this.setState({ DetectObjects: res.data }) })
                .catch(error => console.log(error))
        }
    }


    render() {
        var decs = [];
        for (var i = 0; i < this.state.DetectObjects.length; i++) {
            decs.push(
                <div key={i} style={{
                    color: "blue",
                    fontSize: "15px",
                    textAlign: "center",
                    position: "absolute",
                    border: "3px solid blue",
                    height: this.state.DetectObjects[i].height,
                    left: this.state.DetectObjects[i].left,
                    top: this.state.DetectObjects[i].top,
                    width: this.state.DetectObjects[i].width
                }}>
                    {this.state.DetectObjects[i].name}
                </div>);
        }
        const videoConstraints = {
            facingMode: { exact: "environment" },


        };

        return (

            <div>
                {/*
            <form onSubmit={this.handleSubmitTraining}>
                <input
                    placeholder="path to folder"
                    onChange={this.handleChange}
                    value={this.state.PathFolder}
                    name="PathFolder" />
                <input
                    placeholder="group name"
                    onChange={this.handleChange}
                    value={this.state.GroupName}
                    name="GroupName" />
                <input type="submit" value="add" />
            </form>


            <form onSubmit={this.handleSubmitIdentify}>
                <input
                    placeholder="path to image"
                    onChange={this.handleChange}
                    value={this.state.PathImage}
                    name="PathImage" />           
                <input type="submit" value="identify" />
            </form>*/}
                <div style={{ alignItems: "center", textAlign: "center" }}>
                    <div>
                        <form onSubmit={this.handleOpenImageUrl}>
                            <input
                                style={{ width: "70%" }}
                                placeholder="Image Url"
                                onChange={this.handleChange}
                                value={this.state.ImageUrl}
                                name="ImageUrl" />
                            <input type="submit" value="Ok" />
                        </form>
                        <div>

                            <input style={{ display: "none" }} onChange={this.handleOpenImage} id="file-upload" type="file" />
                            <label for="file-upload" class="custom-file-upload">
                                <img src="https://www.apkmirror.com/wp-content/uploads/2018/06/5b121ade9b2a6.png"
                                    style={{ width: "70px", margin: "20px 20px 20px 20px" }} />
                            </label>


                            <img src="https://lh3.googleusercontent.com/e-qFePY2XsGCRYgvE20kAOIe4GG9DrxbTX9tf5BMTVO5FM_z4KsL6FrqNmYpund_54va=w300"
                                style={{ width: "70px", margin: "20px 20px 20px 20px" }} onClick={this.handleOpenCamera} />
                        </div>
                    </div>
                </div>
                <div style={{ textAlign: "center" }}>
                    <div style={{display:"inline-block"}}>
                        <div style={{ position: "relative" }}>
                            {this.state.isCameraOpen
                                ?
                                <div>
                                    <Camera
                                        videoConstraints={videoConstraints}
                                        height={"auto"}
                                        width={100 + '%'}
                                        ref='webcam'
                                        screenshotFormat="image/jpeg"
                                        screenshotQuality={1.0}

                                    />
                                    <img onClick={this.handleCloseCamera}
                                        style={{ width: "40px", position: "absolute", zIndex: 9999, left: "0%", top: "2%", left: "85%" }}
                                        src="https://image.flaticon.com/icons/png/512/106/106830.png" />
                                    <img style={{ width: "80px", position: "absolute", zIndex: 9999, left: "39%", bottom: "10%" }}
                                        src="https://i.pinimg.com/originals/f9/24/62/f92462126022a3c3ce6700e4f15449dd.png"
                                        onClick={this.handleTakePhoto} /></div>
                                : null}
                        </div>

                        <div className="image" style={{ position: "relative" }}>
                            <img src={this.state.Image} />
                            {decs}
                        </div>
                        {this.state.Image ? <button onClick={this.handleSubmitIdentify2}
                            style={{
                                marginTop: "15px", borderRadius: "25%", padding: "10px 20px", textAlign: "center",
                                display: "inline-block", fontSize: "16px", margin: "4px 2px", cursor: "pointer",
                                transitionDuration: "0.4s", backgroundColor: "white", color: "black",
                                border: "2px solid green"}} >identify</button>
                            : null}
                    </div>
                </div>
            </div>
        );
    }
}
